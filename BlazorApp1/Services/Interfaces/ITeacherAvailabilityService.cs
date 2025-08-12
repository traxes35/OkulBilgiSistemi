using BlazorApp1.Models;

namespace BlazorApp1.Services.Interfaces
{
    public interface ITeacherAvailabilityService
    {
        Task<List<int>> GetSelectedTimeSlotIdsAsync(int userId);
        Task SaveSelectionAsync(int userId, List<int> selectedSlotIds);
    }
}