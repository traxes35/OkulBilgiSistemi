using BlazorApp1.Data;
using BlazorApp1.Models;
using BlazorApp1.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace BlazorApp1.Services
{
    public class TranscriptService : ITranscriptService
    {
        private readonly ApplicationDbContext _context;

        public TranscriptService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task GenerateTranscriptTemplateAsync(int studentId)
        {
            var student = await _context.Users.FindAsync(studentId);
            if (student == null)
                throw new InvalidOperationException("Öğrenci bulunamadı.");

            // Önceki kayıtları sil
            var existing = await _context.TranscriptRecords
                .Where(tr => tr.UserId == studentId)
                .ToListAsync();
            _context.TranscriptRecords.RemoveRange(existing);

            // Term tablosunu sırayla çek (Güz -> Bahar)
            var allTerms = await _context.Terms
                .OrderBy(t => t.Year)
                .ThenBy(t => t.Semester) // 1: Güz, 2: Bahar
                .ToListAsync();

            // Öğrencinin aldığı dersler
            var studentCourses = await _context.StudentCourses
                .Where(sc => sc.UserId == studentId)
                .Include(sc => sc.Course)
                .Include(sc => sc.Term)
                .ToListAsync();

            // Her CourseCode için en son alınanı al
            var latestCourses = studentCourses
                .Where(sc => sc.Course != null && sc.Term != null)
                .GroupBy(sc => sc.Course!.CourseCode)
                .Select(g => g
                    .OrderByDescending(sc => sc.Term.Year)
                    .ThenByDescending(sc => sc.Term.Semester)
                    .First()
                )
                .ToList();

            var transcriptRecords = new List<TranscriptRecord>();

            foreach (var sc in latestCourses)
            {
                if (sc.Course == null || sc.Term == null || sc.Course.ClassLevel == null)
                    continue;

                int semester = sc.Term.Semester;           // Alınan semester (1 veya 2)
                int classLevel = sc.Course.ClassLevel; // 1.sınıf, 2.sınıf, ...

                // Her sınıf 2 term'den oluşur → offset hesapla
                int offset = (classLevel - 1) * 2 + (semester - 1);

                if (offset < 0 || offset >= allTerms.Count)
                    continue;

                var targetTerm = allTerms[offset];

                transcriptRecords.Add(new TranscriptRecord
                {
                    UserId = studentId,
                    CourseId = sc.CourseId,
                    TermId = targetTerm.Id,
                    NumericGrade = sc.Grade,
                    LetterGrade = sc.LetterGrade
                });
            }

            await _context.TranscriptRecords.AddRangeAsync(transcriptRecords);
            await _context.SaveChangesAsync();
        }


        public async Task<List<TranscriptRecord>> GetTranscriptAsync(int studentId)
        {
            var allRecords = await _context.TranscriptRecords
                .Where(r => r.UserId == studentId)
                .Include(r => r.Course)
                .Include(r => r.Term)
                .ToListAsync();

            // 🔹 Geçerli courseCode'lara göre grupla (int olduğu için doğrudan kullanılır)
            var groupedByCourseCode = allRecords
                .Where(r => r.Course != null && r.Course.CourseCode != 0) // courseCode == 0 ise geçersiz
                .GroupBy(r => r.Course!.CourseCode)
                .Select(g => g
                    .OrderByDescending(r => r.Term.Year)
                    .ThenByDescending(r => r.Term.Semester)
                    .First()
                )
                .ToList();

            // 🔹 Placeholder kayıtlar (seçmeli dersler için)
            var placeholderRecords = allRecords
                .Where(r => r.CourseId == null)
                .ToList();

            // 🔹 Listeyi birleştir ve sırala
            return groupedByCourseCode
                .Concat(placeholderRecords)
                .OrderBy(r => r.Term.Year)
                .ThenBy(r => r.Term.Semester)
                .ThenBy(r => r.Course?.CourseName ?? r.PlaceholderName)
                .ToList();
        }

        public async Task<Term?> GetActiveTermAsync()
        {
            return await _context.Terms.FirstOrDefaultAsync(t => t.IsActive);
        }
        public async Task SyncTranscriptWithGradesAsync(int studentId)
        {
            var transcriptRecords = await _context.TranscriptRecords
                .Where(tr => tr.UserId == studentId && tr.CourseId != null)
                .Include(tr => tr.Course)
                .ToListAsync();

            var courseIds = transcriptRecords
                .Where(r => r.CourseId != null)
                .Select(r => r.CourseId!.Value)
                .Distinct()
                .ToList();

            var studentCourses = await _context.StudentCourses
                .Where(sc => sc.UserId == studentId && courseIds.Contains(sc.CourseId))
                .Include(sc => sc.Course)
                .ToListAsync();

            var allLetterGrades = await _context.LetterGradeRanges
                .Where(lr => courseIds.Contains(lr.CourseId))
                .ToListAsync();

            foreach (var record in transcriptRecords)
            {
                var courseId = record.CourseId!.Value;

                var studentCourse = studentCourses.FirstOrDefault(sc => sc.CourseId == courseId);
                if (studentCourse == null || studentCourse.Grade == null || string.IsNullOrWhiteSpace(studentCourse.LetterGrade))
                    continue;

                record.NumericGrade = studentCourse.Grade;
                record.LetterGrade = studentCourse.LetterGrade;

                var coefficient = allLetterGrades
                    .Where(lr => lr.CourseId == courseId)
                    .FirstOrDefault(lr =>
                        lr.Letter.Trim().ToUpper() == studentCourse.LetterGrade.Trim().ToUpper()
                    );

                record.GradeCoefficient = coefficient?.Coefficient;
            }

            await _context.SaveChangesAsync();
        }
    }
}
