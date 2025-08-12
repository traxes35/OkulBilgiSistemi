namespace BlazorApp1.Services.Interfaces
{
    public interface ICourseSelectionService
    {
        Task<bool> IsCourseSelectionOpenAsync();
        Task ToggleCourseSelectionAsync();
    }
}
