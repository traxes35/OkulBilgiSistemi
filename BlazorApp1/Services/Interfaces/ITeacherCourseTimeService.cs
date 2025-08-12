namespace BlazorApp1.Services.Interfaces
{
    public interface ITeacherCourseTimeService
    {
        Task<List<TeacherCourseTime>> GetAllAsync();
        Task<List<TeacherCourseTime>> GetByTeacherIdAsync(int teacherId);
        Task<bool> AssignAsync(TeacherCourseTime assignment);
        Task<bool> RemoveAsync(int id);
        Task RemoveAllByTeacherIdAsync(int teacherId);
        Task<List<TeacherCourseTime>> GetByCourseIdsAsync(List<int> courseIds);
        Task SyncTeacherAssignmentsAsync(int teacherId, List<TeacherCourseTime> newAssignments);

    }

}