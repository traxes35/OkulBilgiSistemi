using BlazorApp1.Models;

namespace BlazorApp1.Services.Interfaces
{
    public interface IStudentCourseService
    {
        Task<List<Course>> GetSelectableCoursesAsync(int studentId, int studentClassLevel);
        Task<List<StudentCourse>> GetSelectedCoursesAsync(int studentId);
        Task SaveSelectedCoursesAsync(int studentId, List<int> courseIds);
        Task<List<StudentCourse>> GetCoursesByStudentIdAsync(int studentId);
        Task<List<Course>> GetActiveCoursesByStudentIdAsync(int studentId);
        Task<List<StudentCourse>> GetDetailedActiveCoursesAsync(int studentId);
        Task<List<int>> GetUnresolvedFfCourseIdsBySameSemesterAsync(int userId);

    }
}
