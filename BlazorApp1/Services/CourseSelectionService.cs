using BlazorApp1.Data;
using BlazorApp1.Models;
using BlazorApp1.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp1.Services
{
    public class CourseSelectionService : ICourseSelectionService
    {
        private readonly ApplicationDbContext _context;

        public CourseSelectionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> IsCourseSelectionOpenAsync()
        {
            var setting = await _context.CourseSelectionSettings.FindAsync(1);
            return setting?.IsSelectionOpen ?? false;
        }

        public async Task ToggleCourseSelectionAsync()
        {
            var setting = await _context.CourseSelectionSettings.FirstOrDefaultAsync(x => x.Id == 1);
            if (setting == null)
            {
                setting = new CourseSelectionSetting { Id = 1, IsSelectionOpen = true };
                _context.CourseSelectionSettings.Add(setting);
            }
            else
            {
                setting.IsSelectionOpen = !setting.IsSelectionOpen;
                _context.CourseSelectionSettings.Update(setting);
            }

            await _context.SaveChangesAsync();
        }
    }
}
