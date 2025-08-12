using BlazorApp1.Data;
using BlazorApp1.Models;

namespace BlazorApp1.Services.Interfaces
{
    public interface ITeacherService
    {
        Task<List<ApplicationUser>> GetAllTeachersAsync();
    }
}
