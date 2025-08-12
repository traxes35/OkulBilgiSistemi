using BlazorApp1.Models;

namespace BlazorApp1.Services.Interfaces
{
    public interface ICourseService
    {
        Task<List<Course>> GetAllCoursesByTermAsync(int termId);
        Task<List<Course>> GetByIdsAsync(List<int> courseIds);
        Task AddCourseAsync(Course course);


    }
}