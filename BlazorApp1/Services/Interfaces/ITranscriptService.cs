using BlazorApp1.Models;

namespace BlazorApp1.Services.Interfaces
{
    public interface ITranscriptService
    {
        Task GenerateTranscriptTemplateAsync(int studentId);
        Task<List<TranscriptRecord>> GetTranscriptAsync(int studentId);
        Task<Term?> GetActiveTermAsync();
        Task SyncTranscriptWithGradesAsync(int studentId);

    }
}
