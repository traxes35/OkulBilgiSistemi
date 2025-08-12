using BlazorApp1.Models;

namespace BlazorApp1.Services.Interfaces
{
    public interface ITeacherCourseService
    {
        // Query
        Task<List<TeacherCourse>> GetAllAsync();
        Task<List<TeacherCourse>> GetCoursesByTeacherIdAsync(int userId);
        Task<List<TeacherCourse>> GetCoursesByTeacherIdAndTermAsync(int teacherId, int termId);

        // Lookups
        Task<TeacherCourse?> GetByIdAsync(int id);
        Task<TeacherCourse?> GetByCourseIdAsync(int courseId);

        // Legacy (composite key desteği – eski kodlar için)
        Task<TeacherCourse?> GetAsync(int userId, int courseId);
        Task DeleteAsync(int userId, int courseId);

        // Mutations
        Task AddAsync(TeacherCourse entity);
        Task UpdateAsync(TeacherCourse entity);

        // Tek aktif atama kuralı
        Task AssignTeacherAsync(int courseId, int teacherId);
        Task ClearAssignmentAsync(int courseId);
    }
}
