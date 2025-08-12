namespace BlazorApp1.Services
{
    using BlazorApp1.Models;
    using BlazorApp1.Data;
    using BlazorApp1.Services.Interfaces;
    using Microsoft.EntityFrameworkCore;

    public class DepartmentService : IDepartmentService
    {
        private readonly ApplicationDbContext _context;

        public DepartmentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Department>> GetAllDepartmentsAsync()
        {
            return await _context.Departments
                .Include(d => d.Faculty)
                .ToListAsync();
        }
    }
}
