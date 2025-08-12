using BlazorApp1.Models;

namespace BlazorApp1.Services.Interfaces
{
    public interface ITermService
    {
        Task<List<Term>> GetAllTermsAsync();
        Task<Term?> GetActiveTermAsync();
        Task<Term?> GetByIdAsync(int id);
        Task AddTermAsync(Term term);
        Task UpdateTermAsync(Term term);
        Task DeleteTermAsync(int id);
        Task<Term?> GetPreviousGuzTermAsync();
    }
}
