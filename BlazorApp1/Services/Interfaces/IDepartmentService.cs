using BlazorApp1.Models;

namespace BlazorApp1.Services.Interfaces
{
    public interface IDepartmentService
    {
        Task<List<Department>> GetAllDepartmentsAsync();
    }
}
