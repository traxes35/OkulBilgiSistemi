using BlazorApp1.Data;
using BlazorApp1.Models;

namespace BlazorApp1.Services.Interfaces
{
    public interface ITeacherGradingService
    {
        Task<List<Course>> GetCoursesByTeacherIdAsync(int teacherId);
        Task<List<ApplicationUser>> GetStudentsByCourseIdAsync(int courseId);
        Task<List<CourseGradeType>> GetCourseGradeTypesAsync(int courseId);
        Task<bool> AddCourseGradeTypeAsync(int courseId, int gradeTypeId, double weight);
        Task SaveStudentGradeAsync(int studentId, int courseGradeTypeId, double score);
        Task<List<GradeType>> GetAllGradeTypesAsync();
        Task UpdateCourseGradeWeightAsync(int courseGradeTypeId, double newWeight);
        Task CalculateAndUpdateFinalGradesAsync(int courseId);
    }
}
