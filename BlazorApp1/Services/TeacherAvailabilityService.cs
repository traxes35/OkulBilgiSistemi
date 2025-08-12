using BlazorApp1.Data;
using BlazorApp1.Models;
using BlazorApp1.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp1.Services
{
    public class TeacherAvailabilityService : ITeacherAvailabilityService
    {
        private readonly ApplicationDbContext _context;

        public TeacherAvailabilityService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<int>> GetSelectedTimeSlotIdsAsync(int userId)
        {
            return await _context.TeacherCourseTimes
                .Where(t => t.UserId == userId)
                .Select(t => t.TimeSlotId)
                .ToListAsync();
        }

        public async Task SaveSelectionAsync(int userId, List<int> selectedSlotIds)
        {
            // 1. Eski verileri sil
            var oldEntries = await _context.TeacherCourseTimes
                .Where(t => t.UserId == userId)
                .ToListAsync();

            _context.TeacherCourseTimes.RemoveRange(oldEntries);

            // 2. Yeni seçimleri ekle
            foreach (var slotId in selectedSlotIds)
            {
                _context.TeacherCourseTimes.Add(new TeacherCourseTime
                {
                    UserId = userId,
                    CourseId = null,
                    TimeSlotId = slotId
                });
            }

            await _context.SaveChangesAsync();
        }
    }
}