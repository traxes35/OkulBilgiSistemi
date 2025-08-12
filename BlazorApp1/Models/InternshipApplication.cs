using BlazorApp1.Data;

namespace BlazorApp1.Models
{
    public class InternshipApplication
    {
        public int Id { get; set; }

        // Öğrenci
        public int StudentId { get; set; }
        public ApplicationUser Student { get; set; } = default!;

        // Opsiyonel: döneme bağlamak istersen
        public int? TermId { get; set; }
        public Term? Term { get; set; }

        public InternshipStatus Status { get; set; } = InternshipStatus.InTeacherReview;

        // Versiyonlamayı takip için
        public int CurrentVersion { get; set; } = 1;

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public ICollection<InternshipDocument> Documents { get; set; } = new List<InternshipDocument>();
        public ICollection<InternshipReview> Reviews { get; set; } = new List<InternshipReview>();
        public ICollection<InternshipMessage> Messages { get; set; } = new List<InternshipMessage>();
    }
}
