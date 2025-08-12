using BlazorApp1.Data;
using BlazorApp1.Models;
using BlazorApp1.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace BlazorApp1.Services
{
    public class InternshipService : IInternshipService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public InternshipService(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // ======================
        // Common / Query
        // ======================
        public async Task<InternshipApplication?> GetApplicationAsync(int applicationId, bool includeRelations = true)
        {
            IQueryable<InternshipApplication> q = _context.InternshipApplications.AsQueryable();

            if (includeRelations)
            {
                q = q.Include(a => a.Student)
                     .Include(a => a.Term)
                     .Include(a => a.Documents)
                     .Include(a => a.Reviews)
                     .Include(a => a.Messages);
            }

            return await q.FirstOrDefaultAsync(a => a.Id == applicationId);
        }

        public async Task<List<InternshipMessage>> GetMessagesAsync(int applicationId)
        {
            return await _context.InternshipMessages
                .Where(m => m.ApplicationId == applicationId)
                .OrderBy(m => m.CreatedAt)
                .ToListAsync();
        }

        public async Task SendMessageAsync(int applicationId, int senderId, bool isToStudent, string body)
        {
            var app = await _context.InternshipApplications.FindAsync(applicationId)
                      ?? throw new InvalidOperationException("Başvuru bulunamadı.");

            if (string.IsNullOrWhiteSpace(body))
                throw new InvalidOperationException("Mesaj boş olamaz.");

            _context.InternshipMessages.Add(new InternshipMessage
            {
                ApplicationId = applicationId,
                SenderId = senderId,
                Body = body.Trim(),
                IsToStudent = isToStudent,
                CreatedAt = DateTime.UtcNow
            });

            app.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        // ======================
        // Student
        // ======================
        public async Task<InternshipApplication> CreateOrGetActiveAsync(int studentId, int? termId = null)
        {
            // Final olmayan mevcut başvuruyu getir
            var existing = await _context.InternshipApplications
                .Where(a => a.StudentId == studentId &&
                            a.Status != InternshipStatus.AdminApproved &&
                            (termId == null || a.TermId == termId))
                .OrderByDescending(a => a.CreatedAt)
                .FirstOrDefaultAsync();

            if (existing != null)
                return existing;

            var app = new InternshipApplication
            {
                StudentId = studentId,
                TermId = termId,
                Status = InternshipStatus.InTeacherReview, // öğrenci yükleyince zaten incelemeye gidecek
                CurrentVersion = 0, // ilk upload'ta 1 olacak
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.InternshipApplications.Add(app);
            await _context.SaveChangesAsync();
            return app;
        }

        public async Task<List<InternshipApplication>> GetMyApplicationsAsync(int studentId)
        {
            return await _context.InternshipApplications
                .Where(a => a.StudentId == studentId)
                .Include(a => a.Documents)
                .Include(a => a.Reviews)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task UploadPdfAsync(int applicationId, IFormFile pdf, CancellationToken ct = default)
        {
            if (pdf == null || pdf.Length == 0)
                throw new InvalidOperationException("PDF bulunamadı.");

            var app = await _context.InternshipApplications
                .Include(a => a.Documents)
                .FirstOrDefaultAsync(a => a.Id == applicationId, ct)
                ?? throw new InvalidOperationException("Başvuru bulunamadı.");

            if (app.Status == InternshipStatus.AdminApproved)
                throw new InvalidOperationException("Onaylanmış başvuruya dosya yüklenemez.");

            // Yalnızca öğrenciye geri döndüğünde veya öğretmen incelemesinde yeni versiyon yüklenebilir
            if (app.Status != InternshipStatus.NeedsFixByStudent &&
                app.Status != InternshipStatus.InTeacherReview)
                throw new InvalidOperationException("Bu durumda belge yüklenemez.");

            // İçerik türü ve uzantı kontrolü
            if (!IsPdf(pdf))
                throw new InvalidOperationException("Sadece PDF yüklenebilir.");

            // Hash hesapla (tekrar yüklemeyi engellemek için)
            string hash;
            using (var ms = new MemoryStream())
            {
                await pdf.CopyToAsync(ms, ct);
                ms.Position = 0;
                hash = ComputeSha256(ms);
                ms.Position = 0;

                // Aynı application içinde aynı hash varsa reddet
                bool existsSameHash = await _context.InternshipDocuments
                    .AnyAsync(d => d.ApplicationId == applicationId && d.ContentHash == hash, ct);

                if (existsSameHash)
                    throw new InvalidOperationException("Aynı içerikte bir PDF zaten yüklenmiş.");

                // Versiyon artır
                app.CurrentVersion += 1;

                // Klasör: wwwroot/uploads/internships/{appId}/
                var baseDir = Path.Combine(_env.WebRootPath, "uploads", "internships", app.Id.ToString());
                Directory.CreateDirectory(baseDir);

                var safeName = MakeSafeFileName(Path.GetFileNameWithoutExtension(pdf.FileName)) + ".pdf";
                var fileName = $"v{app.CurrentVersion}_{safeName}";
                var filePath = Path.Combine(baseDir, fileName);

                // Dosyayı diske yaz
                ms.Position = 0;
                await File.WriteAllBytesAsync(filePath, ms.ToArray(), ct);

                var relPath = Path.Combine("uploads", "internships", app.Id.ToString(), fileName)
                              .Replace(Path.DirectorySeparatorChar, '/');

                var doc = new InternshipDocument
                {
                    ApplicationId = app.Id,
                    Version = app.CurrentVersion,
                    FileName = fileName,
                    FilePath = relPath,
                    ContentHash = hash,
                    FileSize = pdf.Length,
                    UploadedAt = DateTime.UtcNow
                };

                _context.InternshipDocuments.Add(doc);

                // Yükleme sonrası status en az InTeacherReview olsun
                if (app.Status == InternshipStatus.NeedsFixByStudent)
                    app.Status = InternshipStatus.InTeacherReview;

                app.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync(ct);
            }
        }

        // ======================
        // Teacher
        // ======================
        public async Task<List<InternshipApplication>> ListForTeacherAsync(int teacherId)
        {
            // Öğrencisi olduğun başvurular: InTeacherReview + AdminRejectedWaitingTeacher
            var studentIds = await _context.StudentAdvisors
                .Where(sa => sa.TeacherId == teacherId && sa.IsActive)
                .Select(sa => sa.StudentId)
                .ToListAsync();

            var q = _context.InternshipApplications
                .Include(a => a.Student)
                .Include(a => a.Documents)
                .Where(a => (a.Status == InternshipStatus.InTeacherReview ||
                             a.Status == InternshipStatus.AdminRejectedWaitingTeacher) &&
                            studentIds.Contains(a.StudentId))
                .OrderBy(a => a.CreatedAt);

            return await q.ToListAsync();
        }

        public async Task TeacherApproveAsync(int applicationId, int reviewerId)
        {
            var app = await _context.InternshipApplications.FindAsync(applicationId)
                      ?? throw new InvalidOperationException("Başvuru bulunamadı.");

            EnsureTransition(app.Status, InternshipStatus.InAdminReview, allowedFrom: new[]
            {
                InternshipStatus.InTeacherReview
            });

            _context.InternshipReviews.Add(new InternshipReview
            {
                ApplicationId = app.Id,
                ReviewerId = reviewerId,
                ReviewerRole = ReviewerRole.Teacher,
                Decision = ReviewDecision.Approve,
                CreatedAt = DateTime.UtcNow
            });

            app.Status = InternshipStatus.InAdminReview;
            app.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        public async Task TeacherRejectAsync(int applicationId, int reviewerId, string reason)
        {
            var app = await _context.InternshipApplications
                .Include(a => a.Documents)   // eski dosyaları sileceğiz
                .FirstOrDefaultAsync(a => a.Id == applicationId)
                ?? throw new InvalidOperationException("Başvuru bulunamadı.");

            if (string.IsNullOrWhiteSpace(reason))
                throw new InvalidOperationException("Reddetme gerekçesi zorunludur.");

            EnsureTransition(app.Status, InternshipStatus.NeedsFixByStudent, allowedFrom: new[]
            {
        InternshipStatus.InTeacherReview
    });

            // Review kaydı (reddedildi)
            _context.InternshipReviews.Add(new InternshipReview
            {
                ApplicationId = app.Id,
                ReviewerId = reviewerId,
                ReviewerRole = ReviewerRole.Teacher,
                Decision = ReviewDecision.Reject,
                Reason = reason.Trim(),
                CreatedAt = DateTime.UtcNow
            });

            // >>> KRİTİK: Eski dosyaları sil ki versiyon 1’den tekrar başlayabilelim
            if (app.Documents != null && app.Documents.Count > 0)
            {
                _context.InternshipDocuments.RemoveRange(app.Documents);
            }

            // Versiyonu sıfırla, durum öğrenciye iade
            app.CurrentVersion = 0;
            app.Status = InternshipStatus.NeedsFixByStudent;
            app.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        public async Task NotifyStudentAfterAdminRejectAsync(int applicationId, int teacherId, string body)
        {
            var app = await _context.InternshipApplications.FindAsync(applicationId)
                      ?? throw new InvalidOperationException("Başvuru bulunamadı.");

            // Admin reddinden sonra öğretmen öğrenciye bilgi geçer → NeedsFixByStudent
            EnsureTransition(app.Status, InternshipStatus.NeedsFixByStudent, allowedFrom: new[]
            {
                InternshipStatus.AdminRejectedWaitingTeacher
            });

            await SendMessageAsync(applicationId, teacherId, isToStudent: true, body: body);

            app.Status = InternshipStatus.NeedsFixByStudent;
            app.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        // ======================
        // Admin
        // ======================
        public async Task<List<InternshipApplication>> ListForAdminAsync()
        {
            return await _context.InternshipApplications
                .Where(a => a.Status == InternshipStatus.InAdminReview)
                .Include(a => a.Student)
                .Include(a => a.Documents)
                .OrderBy(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task AdminApproveAsync(int applicationId, int reviewerId)
        {

            // 1) Reviewer (Admin) var mı?
            var reviewerExists = await _context.Users.AnyAsync(u => u.Id == reviewerId);
            if (!reviewerExists)
                throw new InvalidOperationException("Geçerli admin kullanıcısı bulunamadı (reviewerId).");

            // 2) Başvuru var mı?
            var app = await _context.InternshipApplications
                .Include(a => a.Documents)
                .FirstOrDefaultAsync(a => a.Id == applicationId)
                ?? throw new InvalidOperationException("Başvuru bulunamadı.");

            // 3) Doğru state mi?
            if (app.Status != InternshipStatus.InAdminReview)
                throw new InvalidOperationException($"Bu durumda onay verilemez: {app.Status}");

            // (Opsiyonel) En az bir belge var mı?
            if (app.Documents == null || app.Documents.Count == 0)
                throw new InvalidOperationException("Belgesiz başvuru onaylanamaz.");

            // 4) Review kaydı
            _context.InternshipReviews.Add(new InternshipReview
            {
                ApplicationId = app.Id,
                ReviewerId = reviewerId,
                ReviewerRole = ReviewerRole.Admin,
                Decision = ReviewDecision.Approve,
                Reason = null, // approve için boş
                CreatedAt = DateTime.UtcNow
            });

            // 5) Final onay
            app.Status = InternshipStatus.AdminApproved;
            app.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                // Debug kolaylığı: inner exception’ı yüzeye taşı
                var inner = ex.InnerException?.Message ?? ex.Message;
                throw new InvalidOperationException("Kaydetme sırasında hata: " + inner, ex);
            }
        }


        public async Task AdminRejectAsync(int applicationId, int reviewerId, string reason)
        {
            var app = await _context.InternshipApplications.FindAsync(applicationId)
                      ?? throw new InvalidOperationException("Başvuru bulunamadı.");

            if (string.IsNullOrWhiteSpace(reason))
                throw new InvalidOperationException("Reddetme gerekçesi zorunludur.");

            // Zaten admin tarafından reddedilmişse: net mesaj ver ve çık
            if (app.Status == InternshipStatus.AdminRejectedWaitingTeacher)
                throw new InvalidOperationException("Bu başvuru zaten admin tarafından reddedildi. Öğretmenin öğrenciye bilgilendirme yapması bekleniyor.");

            // Sadece InAdminReview iken redde izin ver
            EnsureTransition(app.Status, InternshipStatus.AdminRejectedWaitingTeacher, allowedFrom: new[]
            {
        InternshipStatus.InAdminReview
    });

            _context.InternshipReviews.Add(new InternshipReview
            {
                ApplicationId = app.Id,
                ReviewerId = reviewerId,
                ReviewerRole = ReviewerRole.Admin,
                Decision = ReviewDecision.Reject,
                Reason = reason.Trim(),
                CreatedAt = DateTime.UtcNow
            });

            app.Status = InternshipStatus.AdminRejectedWaitingTeacher;
            app.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }


        // ======================
        // Helpers
        // ======================
        private static void EnsureTransition(InternshipStatus current, InternshipStatus target, InternshipStatus[] allowedFrom)
        {
            if (!allowedFrom.Contains(current))
                throw new InvalidOperationException($"Geçişe izin yok: {current} → {target}");
        }

        private static bool IsPdf(IFormFile file)
        {
            // Basit kontrol: content-type ve uzantı
            var okContentType = file.ContentType?.Equals("application/pdf", StringComparison.OrdinalIgnoreCase) == true
                                || file.ContentType?.Equals("application/octet-stream", StringComparison.OrdinalIgnoreCase) == true;
            var okExt = Path.GetExtension(file.FileName).Equals(".pdf", StringComparison.OrdinalIgnoreCase);
            return okContentType && okExt;
        }

        private static string ComputeSha256(Stream s)
        {
            using var sha = SHA256.Create();
            var hash = sha.ComputeHash(s);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }

        private static string MakeSafeFileName(string input)
        {
            // Dosya adı sadeleştir
            var safe = Regex.Replace(input, @"[^\w\-]+", "_", RegexOptions.Compiled);
            if (string.IsNullOrWhiteSpace(safe))
                safe = "document";
            return safe.Length > 60 ? safe[..60] : safe;
        }

        public async Task ReplacePdfAsync(int applicationId, int targetVersion, IFormFile pdf, int studentId, CancellationToken ct = default)
        {
            if (pdf == null || pdf.Length == 0) throw new InvalidOperationException("PDF bulunamadı.");
            if (!IsPdf(pdf)) throw new InvalidOperationException("Sadece PDF yüklenebilir.");

            var app = await _context.InternshipApplications
                .Include(a => a.Documents)
                .FirstOrDefaultAsync(a => a.Id == applicationId, ct)
                ?? throw new InvalidOperationException("Başvuru bulunamadı.");

            if (app.StudentId != studentId) throw new InvalidOperationException("Yetkisiz işlem.");
            if (app.Status == InternshipStatus.AdminApproved) throw new InvalidOperationException("Onaylı başvuruda değişiklik yapılamaz.");
            if (app.Status != InternshipStatus.NeedsFixByStudent && app.Status != InternshipStatus.InTeacherReview)
                throw new InvalidOperationException("Bu durumda belge değiştirilemez.");

            var doc = app.Documents.FirstOrDefault(d => d.Version == targetVersion)
                      ?? throw new InvalidOperationException($"v{targetVersion} bulunamadı.");

            // yeni hash
            string hash;
            using var ms = new MemoryStream();
            await pdf.CopyToAsync(ms, ct);
            ms.Position = 0;
            hash = ComputeSha256(ms);

            // aynı application içinde aynı hash olmasın (hedef belge hariç)
            bool dup = await _context.InternshipDocuments
                .AnyAsync(d => d.ApplicationId == app.Id && d.ContentHash == hash && d.Id != doc.Id, ct);
            if (dup) throw new InvalidOperationException("Aynı içerikte bir PDF zaten mevcut.");

            // eski dosyayı sil (fiziksel)
            TryDeletePhysical(doc.FilePath);

            // yeni dosyayı yaz
            var baseDir = Path.Combine(_env.WebRootPath, "uploads", "internships", app.Id.ToString());
            Directory.CreateDirectory(baseDir);

            var safeName = MakeSafeFileName(Path.GetFileNameWithoutExtension(pdf.FileName)) + ".pdf";
            var fileName = $"v{doc.Version}_{safeName}";
            var filePath = Path.Combine(baseDir, fileName);

            ms.Position = 0;
            await File.WriteAllBytesAsync(filePath, ms.ToArray(), ct);

            var relPath = Path.Combine("uploads", "internships", app.Id.ToString(), fileName)
                          .Replace(Path.DirectorySeparatorChar, '/');

            // doc kaydını güncelle (versiyon aynı kalır)
            doc.FileName = fileName;
            doc.FilePath = relPath;
            doc.ContentHash = hash;
            doc.FileSize = pdf.Length;
            doc.UploadedAt = DateTime.UtcNow;

            // <<< EKLENEN KISIM: NeedsFixByStudent ise öğretmen incelemesine geri dön >>>
            if (app.Status == InternshipStatus.NeedsFixByStudent)
                app.Status = InternshipStatus.InTeacherReview;

            app.UpdatedAt = DateTime.UtcNow;

            try { await _context.SaveChangesAsync(ct); }
            catch (DbUpdateException ex)
            {
                var inner = ex.InnerException?.Message ?? ex.Message;
                throw new InvalidOperationException("Kaydetme sırasında hata: " + inner, ex);
            }
        }


        public async Task DeletePdfAsync(int applicationId, int targetVersion, int studentId, CancellationToken ct = default)
        {   
            var app = await _context.InternshipApplications
                .Include(a => a.Documents)
                .FirstOrDefaultAsync(a => a.Id == applicationId, ct)
                ?? throw new InvalidOperationException("Başvuru bulunamadı.");

            if (app.StudentId != studentId) throw new InvalidOperationException("Yetkisiz işlem.");
            if (app.Status == InternshipStatus.AdminApproved) throw new InvalidOperationException("Onaylı başvuruda silinemez.");
            if (app.Status != InternshipStatus.NeedsFixByStudent && app.Status != InternshipStatus.InTeacherReview)
                throw new InvalidOperationException("Bu durumda belge silinemez.");

            if (app.CurrentVersion != targetVersion)
                throw new InvalidOperationException("Sadece son (en güncel) versiyon silinebilir.");

            var doc = app.Documents.FirstOrDefault(d => d.Version == targetVersion)
                      ?? throw new InvalidOperationException($"v{targetVersion} bulunamadı.");

            // fiziksel dosyayı sil
            TryDeletePhysical(doc.FilePath);

            // kaydı sil
            _context.InternshipDocuments.Remove(doc);

            // CurrentVersion'ı geriye çek
            app.CurrentVersion = Math.Max(0, app.CurrentVersion - 1);
            app.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(ct);
        }

        // helper
        private void TryDeletePhysical(string relPath)
        {
            if (string.IsNullOrWhiteSpace(relPath)) return;
            var full = Path.Combine(_env.WebRootPath, relPath.Replace('/', Path.DirectorySeparatorChar));
            try { if (File.Exists(full)) File.Delete(full); } catch { /* loglamak istersen ekle */ }
        }

    }
}
