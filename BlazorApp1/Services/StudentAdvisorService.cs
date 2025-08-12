using BlazorApp1.Data;
using BlazorApp1.Models;
using BlazorApp1.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp1.Services
{
    public class StudentAdvisorService : IStudentAdvisorService
    {
        private readonly ApplicationDbContext _context;

        public StudentAdvisorService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Aktif danışman öğretmeni getir
        public async Task<ApplicationUser?> GetActiveAdvisorAsync(int studentId)
        {
            var advisor = await _context.StudentAdvisors
                .Include(sa => sa.Teacher)
                .FirstOrDefaultAsync(sa => sa.StudentId == studentId && sa.IsActive);

            return advisor?.Teacher;
        }
    }
}
