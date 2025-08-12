using BlazorApp1.Data;
using BlazorApp1.Models;
using BlazorApp1.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp1.Services
{
    public class CourseService : ICourseService
    {
        private readonly ApplicationDbContext _context;

        public CourseService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Course>> GetAllCoursesByTermAsync(int termId)
        {
            return await _context.Courses
                .Include(c => c.Department)
                .Include(c => c.Term)
                .Include(c => c.TeacherCourses)
                .Where(c => c.TermId == termId)
                .ToListAsync();
        }

        public async Task<List<Course>> GetByIdsAsync(List<int> courseIds)
        {
            return await _context.Courses
                .Where(c => courseIds.Contains(c.Id))
                .ToListAsync();
        }

        public async Task AddCourseAsync(Course course)
        {
            _context.Courses.Add(course);
            await _context.SaveChangesAsync();
        }
    }
}
