using BlazorApp1.Models;

namespace BlazorApp1.Services.Interfaces
{
    public interface IStudentGradeService
    {
        Task<List<StudentGrade>> GetGradesByStudentIdAsync(int studentId);
        Task<List<StudentGrade>> GetStudentGradesByCourseIdAsync(int courseId);

    }
}