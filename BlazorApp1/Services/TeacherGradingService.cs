using BlazorApp1.Data;
using BlazorApp1.Models;
using BlazorApp1.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp1.Services
{
    public class TeacherGradingService : ITeacherGradingService
    {
        private readonly ApplicationDbContext _context;

        public TeacherGradingService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<List<GradeType>> GetAllGradeTypesAsync()
        {
            return await _context.GradeTypes.ToListAsync();
        }
        public async Task<List<Course>> GetCoursesByTeacherIdAsync(int teacherId)
        {
            // Aktif TermId'yi çek  
            var activeTerm = await _context.Terms.FirstOrDefaultAsync(t => t.IsActive);
            if (activeTerm == null)
                return new List<Course>();

            return await _context.TeacherCourses
                .Where(tc => tc.UserId == teacherId && tc.Course.TermId == activeTerm.Id)
                .Select(tc => tc.Course)
                .Distinct()
                .ToListAsync();
        }

        public async Task<List<ApplicationUser>> GetStudentsByCourseIdAsync(int courseId)
        {
            var userIds = await _context.StudentCourses
                .Where(sc => sc.CourseId == courseId)
                .Select(sc => sc.UserId)
                .Distinct()
                .ToListAsync();

            return await _context.Users
                .Where(u => userIds.Contains(u.Id))
                .ToListAsync();
        }

        public async Task<List<CourseGradeType>> GetCourseGradeTypesAsync(int courseId)
        {
            return await _context.CourseGradeTypes
                .Include(cgt => cgt.GradeType)
                .Where(cgt => cgt.CourseId == courseId)
                .ToListAsync();
        }

        public async Task<bool> AddCourseGradeTypeAsync(int courseId, int gradeTypeId, double weight)
        {
            // Aynı gradeType zaten bu derste varsa tekrar ekleme
            bool exists = await _context.CourseGradeTypes
                .AnyAsync(cgt => cgt.CourseId == courseId && cgt.GradeTypeId == gradeTypeId);

            if (exists)
                return false;

            var newEntry = new CourseGradeType
            {
                CourseId = courseId,
                GradeTypeId = gradeTypeId,
                Weight = weight,
                CreatedAt = DateTime.UtcNow
            };

            _context.CourseGradeTypes.Add(newEntry);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task SaveStudentGradeAsync(int studentId, int courseGradeTypeId, double score)
        {
            // Var olan notu al
            var existingGrade = await _context.StudentGrades
                .FirstOrDefaultAsync(g =>
                    g.UserId == studentId &&
                    g.CourseGradeTypeId == courseGradeTypeId);

            if (existingGrade != null)
            {
                // ❌ 24 saat kuralı kaldırıldı: zaman kontrolü yok
                existingGrade.Score = score;
                existingGrade.VersionNumber += 1;
                existingGrade.Date = DateTime.UtcNow;
                existingGrade.IsDraft = false;

                // EF izlediği için sadece SaveChanges yeterli
            }
            else
            {
                // Yeni not kaydı
                var grade = new StudentGrade
                {
                    UserId = studentId,
                    CourseGradeTypeId = courseGradeTypeId,
                    Score = score,
                    VersionNumber = 1,
                    Date = DateTime.UtcNow,
                    IsDraft = false
                };

                _context.StudentGrades.Add(grade);
            }

            await _context.SaveChangesAsync();
        }
        public async Task UpdateCourseGradeWeightAsync(int courseGradeTypeId, double newWeight)
        {
            var item = await _context.CourseGradeTypes.FindAsync(courseGradeTypeId);
            if (item == null) return;

            item.Weight = newWeight;
            await _context.SaveChangesAsync();
        }
        public async Task CalculateAndUpdateFinalGradesAsync(int courseId)
        {
            var courseGradeTypes = await _context.CourseGradeTypes
                .Include(cgt => cgt.GradeType)
                .Where(cgt => cgt.CourseId == courseId)
                .ToListAsync();

            if (courseGradeTypes.Sum(x => x.Weight) != 100)
                throw new InvalidOperationException("Ağırlıkların toplamı %100 değil!");

            // ❗Eğer LetterGradeRanges yoksa, otomatik ekle
            bool hasLetterRanges = await _context.LetterGradeRanges.AnyAsync(lr => lr.CourseId == courseId);
            if (!hasLetterRanges)
            {
                var defaultRanges = new List<LetterGradeRange>
        {
            new() { CourseId = courseId, Letter = "AA", LowerBound = 90, UpperBound = 100, Coefficient = 4.0 },
            new() { CourseId = courseId, Letter = "BA", LowerBound = 85, UpperBound = 89, Coefficient = 3.5 },
            new() { CourseId = courseId, Letter = "BB", LowerBound = 80, UpperBound = 84, Coefficient = 3.0 },
            new() { CourseId = courseId, Letter = "CB", LowerBound = 75, UpperBound = 79, Coefficient = 2.5 },
            new() { CourseId = courseId, Letter = "CC", LowerBound = 70, UpperBound = 74, Coefficient = 2.0 },
            new() { CourseId = courseId, Letter = "DC", LowerBound = 65, UpperBound = 69, Coefficient = 1.5 },
            new() { CourseId = courseId, Letter = "DD", LowerBound = 60, UpperBound = 64, Coefficient = 1.0 },
            new() { CourseId = courseId, Letter = "FD", LowerBound = 50, UpperBound = 59, Coefficient = 0.5 },
            new() { CourseId = courseId, Letter = "FF", LowerBound = 0,  UpperBound = 49, Coefficient = 0.0 },
        };

                await _context.LetterGradeRanges.AddRangeAsync(defaultRanges);
                await _context.SaveChangesAsync();
            }
            var activeTerm = await _context.Terms.FirstOrDefaultAsync(t => t.IsActive);
            var students = await _context.StudentCourses
                .Include(sc => sc.Course)
                .Where(sc => sc.CourseId == courseId && sc.Course.TermId == activeTerm.Id)
                .ToListAsync();

            var studentGrades = await _context.StudentGrades
                .Where(sg => sg.CourseGradeType.CourseId == courseId)
                .ToListAsync();

            foreach (var student in students)
            {
                double total = 0;

                foreach (var cgt in courseGradeTypes)
                {
                    var grade = studentGrades
                        .FirstOrDefault(g => g.UserId == student.UserId && g.CourseGradeTypeId == cgt.Id);

                    if (grade != null)
                        total += (grade.Score * cgt.Weight) / 100.0;
                }

                if (total > 0)
                {
                    student.Grade = (int)Math.Round(total);

                    var letter = await _context.LetterGradeRanges
                        .Where(lr => lr.CourseId == courseId &&
                                     lr.LowerBound <= student.Grade && student.Grade <= lr.UpperBound)
                        .OrderByDescending(lr => lr.LowerBound)
                        .Select(lr => lr.Letter)
                        .FirstOrDefaultAsync();

                    student.LetterGrade = letter;
                }
                else
                {
                    student.Grade = null;
                    student.LetterGrade = null;
                }

                student.IsActive = false; // 🔴 Final sonrası dersi pasif yap
            }

            await _context.SaveChangesAsync();
        }

    }

}
