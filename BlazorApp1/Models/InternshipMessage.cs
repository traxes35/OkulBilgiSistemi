using BlazorApp1.Data;

namespace BlazorApp1.Models
{
    public class InternshipMessage
    {
        public int Id { get; set; }
        public int ApplicationId { get; set; }
        public InternshipApplication Application { get; set; } = default!;

        public int SenderId { get; set; }                // Student/Teacher/Admin kullanıcı Id
        public ApplicationUser Sender { get; set; } = default!;

        public string Body { get; set; } = default!;
        public DateTime CreatedAt { get; set; }

        // Kolay filtre için: mesaj öğrenci hedefli mi?
        public bool IsToStudent { get; set; }
    }
}
