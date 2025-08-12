using BlazorApp1.Data;
using BlazorApp1.Models;
using BlazorApp1.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp1.Services
{
    public class StudentGradeService : IStudentGradeService
    {
        private readonly ApplicationDbContext _context;

        public StudentGradeService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<StudentGrade>> GetGradesByStudentIdAsync(int studentId)
        {
            return await _context.StudentGrades
                .Include(g => g.CourseGradeType)
                .ThenInclude(cgt => cgt.GradeType)
                .Where(g => g.UserId == studentId && !g.IsDraft)
                .ToListAsync();
        }
        public async Task<List<StudentGrade>> GetStudentGradesByCourseIdAsync(int courseId)
        {
            return await _context.StudentGrades
                .Include(g => g.CourseGradeType)
                .Where(g => g.CourseGradeType.CourseId == courseId)
                .ToListAsync();
        }
    }
}