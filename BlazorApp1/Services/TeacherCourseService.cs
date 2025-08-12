using BlazorApp1.Data;
using BlazorApp1.Models;
using BlazorApp1.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp1.Services
{
    public class TeacherCourseService : ITeacherCourseService
    {
        private readonly ApplicationDbContext _context;
        public TeacherCourseService(ApplicationDbContext context) => _context = context;

        public async Task<List<TeacherCourse>> GetCoursesByTeacherIdAsync(int userId)
        {
            return await _context.TeacherCourses
                .Include(tc => tc.Course)
                .Where(tc => tc.UserId == userId)
                .ToListAsync();
        }

        public async Task<List<TeacherCourse>> GetAllAsync()
        {
            return await _context.TeacherCourses
                .Include(tc => tc.Course)
                .ToListAsync();
        }

        public async Task<TeacherCourse?> GetByIdAsync(int id)
            => await _context.TeacherCourses
                .Include(tc => tc.Course)
                .FirstOrDefaultAsync(tc => tc.Id == id);

        public async Task<TeacherCourse?> GetByCourseIdAsync(int courseId)
            => await _context.TeacherCourses
                .Include(tc => tc.Course)
                .Where(tc => tc.CourseId == courseId)
                .OrderByDescending(tc => tc.IsActive)
                .ThenByDescending(tc => tc.Id)
                .FirstOrDefaultAsync();

        // Legacy lookup (composite key)
        public async Task<TeacherCourse?> GetAsync(int userId, int courseId)
            => await _context.TeacherCourses
                .Include(tc => tc.Course)
                .FirstOrDefaultAsync(tc => tc.UserId == userId && tc.CourseId == courseId);

        // Legacy delete (composite key)
        public async Task DeleteAsync(int userId, int courseId)
        {
            var entity = await _context.TeacherCourses
                .FirstOrDefaultAsync(tc => tc.UserId == userId && tc.CourseId == courseId);
            if (entity != null)
            {
                _context.TeacherCourses.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task AddAsync(TeacherCourse entity)
        {
            _context.TeacherCourses.Add(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(TeacherCourse entity)
        {
            var existing = await _context.TeacherCourses
                .FirstOrDefaultAsync(x => x.Id == entity.Id);

            if (existing != null)
            {
                existing.IsActive = entity.IsActive;
                existing.IsResponsibilityApproved = entity.IsResponsibilityApproved;
                // Gerekirse UserId/CourseId da güncellenebilir
                await _context.SaveChangesAsync();
            }
        }

        // Upsert: aynı derse tek aktif atama
        public async Task AssignTeacherAsync(int courseId, int teacherId)
        {
            var allForCourse = await _context.TeacherCourses
                .Where(tc => tc.CourseId == courseId)
                .ToListAsync();

            foreach (var tc in allForCourse)
                tc.IsActive = false;

            var current = allForCourse.FirstOrDefault(tc => tc.UserId == teacherId);
            if (current is null)
            {
                current = new TeacherCourse
                {
                    CourseId = courseId,
                    UserId = teacherId,
                    IsActive = true,
                    IsResponsibilityApproved = false
                };
                _context.TeacherCourses.Add(current);
            }
            else
            {
                current.IsActive = true;
                current.IsResponsibilityApproved = false;
                _context.TeacherCourses.Update(current);
            }

            await _context.SaveChangesAsync();
        }

        public async Task ClearAssignmentAsync(int courseId)
        {
            var allForCourse = await _context.TeacherCourses
                .Where(tc => tc.CourseId == courseId)
                .ToListAsync();

            if (allForCourse.Count == 0) return;

            _context.TeacherCourses.RemoveRange(allForCourse);
            await _context.SaveChangesAsync();
        }

        public async Task<List<TeacherCourse>> GetCoursesByTeacherIdAndTermAsync(int teacherId, int termId)
        {
            return await _context.TeacherCourses
                .Include(tc => tc.Course)
                .Where(tc => tc.UserId == teacherId && tc.Course.TermId == termId)
                .ToListAsync();
        }
    }
}
