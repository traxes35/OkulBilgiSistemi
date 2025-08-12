using BlazorApp1.Models;

namespace BlazorApp1.Services.Interfaces
{
    public interface IInternshipService
    {
        // ---- Common / Query ----
        Task<InternshipApplication?> GetApplicationAsync(int applicationId, bool includeRelations = true);
        Task<List<InternshipMessage>> GetMessagesAsync(int applicationId);
        Task SendMessageAsync(int applicationId, int senderId, bool isToStudent, string body);

        // ---- Student ----
        Task<InternshipApplication> CreateOrGetActiveAsync(int studentId, int? termId = null);
        Task<List<InternshipApplication>> GetMyApplicationsAsync(int studentId);
        Task UploadPdfAsync(int applicationId, IFormFile pdf, CancellationToken ct = default);

        // ---- Teacher ----
        Task<List<InternshipApplication>> ListForTeacherAsync(int teacherId);
        Task TeacherApproveAsync(int applicationId, int reviewerId);
        Task TeacherRejectAsync(int applicationId, int reviewerId, string reason);
        Task NotifyStudentAfterAdminRejectAsync(int applicationId, int teacherId, string body);

        // ---- Admin ----
        Task<List<InternshipApplication>> ListForAdminAsync();
        Task AdminApproveAsync(int applicationId, int reviewerId);
        Task AdminRejectAsync(int applicationId, int reviewerId, string reason);
        // IInternshipService.cs
        Task ReplacePdfAsync(int applicationId, int targetVersion, IFormFile pdf, int studentId, CancellationToken ct = default);
        Task DeletePdfAsync(int applicationId, int targetVersion, int studentId, CancellationToken ct = default);

    }
}
