namespace BlazorApp1.Models
{
    public class InternshipDocument
    {
        public int Id { get; set; }
        public int ApplicationId { get; set; }
        public InternshipApplication Application { get; set; } = default!;

        public int Version { get; set; } // 1,2,3...
        public string FileName { get; set; } = default!;
        public string FilePath { get; set; } = default!;  // wwwroot/uploads/internships/{appId}/v{version}.pdf
        public string ContentHash { get; set; } = default!; // SHA-256 vb.
        public long FileSize { get; set; }
        public DateTime UploadedAt { get; set; }
    }
}
