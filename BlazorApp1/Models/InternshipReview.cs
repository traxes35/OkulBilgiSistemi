using BlazorApp1.Data;

namespace BlazorApp1.Models
{
    public class InternshipReview
    {
        public int Id { get; set; }
        public int ApplicationId { get; set; }
        public InternshipApplication Application { get; set; } = default!;

        public int ReviewerId { get; set; }              // Teacher veya Admin kullanıcı Id (AspNetUsers.Id)
        public ApplicationUser Reviewer { get; set; } = default!;

        public ReviewerRole ReviewerRole { get; set; }   // Teacher/Admin
        public ReviewDecision Decision { get; set; }     // Approve/Reject

        public string? Reason { get; set; }              // Reject ise zorunlu (uygulama tarafında zorla)
        public DateTime CreatedAt { get; set; }
    }
}
