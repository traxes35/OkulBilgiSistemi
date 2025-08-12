using BlazorApp1.Data;
using BlazorApp1.Models;
using BlazorApp1.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp1.Services
{
    public class TimeSlotService : ITimeSlotService
    {
        private readonly ApplicationDbContext _context;

        public TimeSlotService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<TimeSlot>> GetAllAsync()
        {
            return await _context.TimeSlots
                .OrderBy(ts => ts.Day) // Gün sırasına göre sıralayabilirsin
                .ThenBy(ts => ts.StartTime)
                .ToListAsync();
        }
    }
}