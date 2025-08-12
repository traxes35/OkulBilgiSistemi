using BlazorApp1.Data;

namespace BlazorApp1.Models
{
    public class StudentAdvisor
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public ApplicationUser Student { get; set; } = default!;

        public int TeacherId { get; set; }
        public ApplicationUser Teacher { get; set; } = default!;

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
    }
}
