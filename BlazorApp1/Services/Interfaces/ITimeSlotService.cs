using BlazorApp1.Models;

namespace BlazorApp1.Services.Interfaces
{
    public interface ITimeSlotService
    {
        Task<List<TimeSlot>> GetAllAsync();
    }
}