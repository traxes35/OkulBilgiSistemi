using BlazorApp1.Models;

namespace BlazorApp1.Services.Interfaces
{
    public interface IClassroomService
    {
        Task<List<Classroom>> GetAllAsync();
        Task<Classroom?> GetByIdAsync(int id);
        Task AddAsync(Classroom classroom);
        Task UpdateAsync(Classroom classroom);
        Task DeleteAsync(int id);
        Task<Classroom?> FindAvailableClassroomAsync(List<TimeSlot> slots, List<Classroom> classrooms);
        Task<List<Classroom>> GetByDepartmentAsync(int departmentId);
    }
}