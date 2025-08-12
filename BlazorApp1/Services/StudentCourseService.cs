using BlazorApp1.Data;
using BlazorApp1.Models;
using BlazorApp1.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp1.Services
{
    public class StudentCourseService : IStudentCourseService
    {
        private readonly ApplicationDbContext _context;

        public StudentCourseService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Course>> GetSelectableCoursesAsync(int studentId, int studentClassLevel)
        {
            var activeTerm = await _context.Terms.FirstOrDefaultAsync(t => t.IsActive);
            if (activeTerm == null)
                return new List<Course>();

            var studentGrades = await _context.StudentCourses
                .Include(sc => sc.Course)
                .Where(sc => sc.UserId == studentId)
                .ToListAsync();

            // 🔥 FF alınan alt sınıf derslerin CourseCode’larını al
            var mustTakeCourseCodes = studentGrades
                .Where(sc => sc.LetterGrade == "FF" && sc.Course.ClassLevel < studentClassLevel)
                .Select(sc => sc.Course.CourseCode)
                .Distinct()
                .ToHashSet();

            // 🔁 Aktif dönemde açılan ve bu FF kodlarına sahip olan dersler
            var activeFFCourses = await _context.Courses
                .Where(c =>
                    c.TermId == activeTerm.Id &&
                    mustTakeCourseCodes.Contains(c.CourseCode))
                .ToListAsync();

            // ✅ Onaylı açılan tüm dersleri al
            var approvedCourses = await _context.TeacherCourses
                .Where(tc => (bool)tc.IsResponsibilityApproved && tc.Course.TermId == activeTerm.Id)
                .Select(tc => tc.Course)
                .Distinct()
                .ToListAsync();

            // 🎯 Tüm onaylı dersleri seçilebilir yap (sınıf farkı gözetmeden)
            var selectableCourses = approvedCourses
                .Where(course =>
                    course.ClassLevel <= studentClassLevel || // Kendi sınıfı ve alt sınıflar
                    (studentClassLevel != 1 && course.ClassLevel > studentClassLevel)) // Üst sınıflar (1. sınıf hariç)
                .ToList();

            // 🔗 FF tekrarları zaten selectableCourses içinde olabilir, ama emin olmak için ekle
            var result = selectableCourses
                .Concat(activeFFCourses)
                .DistinctBy(c => c.Id)
                .ToList();

            return result;
        }


        public async Task<List<StudentCourse>> GetSelectedCoursesAsync(int studentId)
        {
            var activeTerm = await _context.Terms.FirstOrDefaultAsync(t => t.IsActive);
            if (activeTerm == null)
                return new List<StudentCourse>();

            return await _context.StudentCourses
                .Include(sc => sc.Course)
                .Where(sc => sc.UserId == studentId && sc.Course.TermId == activeTerm.Id)
                .ToListAsync();
        }
        public async Task SaveSelectedCoursesAsync(int studentId, List<int> newCourseIds)
        {
            var activeTerm = await _context.Terms.FirstOrDefaultAsync(t => t.IsActive);
            if (activeTerm == null) return;

            // 1. Öğrencinin mevcut aktif dönem kayıtları
            var existing = await _context.StudentCourses
                .Where(sc => sc.UserId == studentId && sc.TermId == activeTerm.Id)
                .ToListAsync();

            // 2. Ders seçiminden çıkarılan dersler (artık seçilmeyenler)
            var removedCourses = existing
                .Where(sc => !newCourseIds.Contains(sc.CourseId))
                .ToList();

            foreach (var removed in removedCourses)
            {
                removed.IsActive = false;

                var course = await _context.Courses.FindAsync(removed.CourseId);
                if (course != null)
                {
                    course.Quota += 1;
                    _context.Courses.Update(course);
                }

                _context.StudentCourses.Update(removed);
            }

            // 3. Yeni eklenen dersleri bul (önceden hiç alınmamış)
            var addedCourseIds = newCourseIds
                .Where(cid => !existing.Any(e => e.CourseId == cid))
                .ToList();

            foreach (var courseId in addedCourseIds)
            {
                var course = await _context.Courses.FindAsync(courseId);
                if (course == null || course.Quota <= 0) continue;

                var newSC = new StudentCourse
                {
                    UserId = studentId,
                    CourseId = courseId,
                    TermId = activeTerm.Id,
                    IsActive = true // 🔥 Aktif ders
                };

                course.Quota -= 1;
                _context.Courses.Update(course);
                _context.StudentCourses.Add(newSC);
            }

            // 4. Hâlihazırda sistemde olan ama tekrar seçilmiş dersleri aktif hale getir
            foreach (var existingCourse in existing.Where(sc => newCourseIds.Contains(sc.CourseId)))
            {
                existingCourse.IsActive = true;
                _context.StudentCourses.Update(existingCourse);
            }

            await _context.SaveChangesAsync();
        }
        public async Task<List<StudentCourse>> GetCoursesByStudentIdAsync(int studentId)
        {
            return await _context.StudentCourses
                .Include(sc => sc.Course)
                    .ThenInclude(c => c.Term) // 🔴 bu şart
                .Where(sc => sc.UserId == studentId)
                .ToListAsync();
        }
        public async Task<List<Course>> GetActiveCoursesByStudentIdAsync(int studentId)
        {
            return await _context.StudentCourses
                .Include(sc => sc.Course)
                .Where(sc => sc.UserId == studentId && sc.LetterGrade == null)
                .Select(sc => sc.Course)
                .ToListAsync();
        }
        public async Task<List<StudentCourse>> GetDetailedActiveCoursesAsync(int studentId)
        {
            var activeTerm = await _context.Terms.FirstOrDefaultAsync(t => t.IsActive);
            return await _context.StudentCourses
    .Where(sc => sc.UserId == studentId && sc.Course.TermId == activeTerm.Id)
    .Include(sc => sc.Course)
    .Include(sc => sc.Course.Department)
    .ToListAsync(); ;
        }
        public async Task<List<int>> GetUnresolvedFfCourseIdsBySameSemesterAsync(int userId)
        {
            var student = await _context.Users.FindAsync(userId);
            var studentClassLevel = student?.SinifSeviyesi ?? 1;

            var activeTerm = await _context.Terms.FirstOrDefaultAsync(t => t.IsActive);
            var activeSemester = activeTerm?.Semester;

            if (activeSemester == null)
                return new List<int>();

            var unresolved = await _context.StudentCourses
                .Where(sc => sc.UserId == userId &&
                             sc.LetterGrade == "FF" &&
                             sc.Term != null &&
                             sc.Term.Semester == activeSemester &&
                             sc.Course.ClassLevel < studentClassLevel &&
                             !_context.StudentCourses.Any(s =>
                                 s.UserId == userId &&
                                 s.CourseId == sc.CourseId &&
                                 string.IsNullOrEmpty(s.LetterGrade)))
                .Select(sc => sc.CourseId)
                .Distinct()
                .ToListAsync();

            return unresolved;
        }


    }
}