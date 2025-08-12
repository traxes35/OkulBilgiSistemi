using BlazorApp1.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorApp1.Models
{
    public class TranscriptRecord
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }
        public ApplicationUser Student { get; set; } = null!;

        [Required]
        public int TermId { get; set; }
        public Term Term { get; set; } = null!;

        public int? CourseId { get; set; }  // Seçmeli ders seçilmediyse null
        public Course? Course { get; set; }

        public string? PlaceholderName { get; set; } // "Seçmeli 1" gibi

        public int? NumericGrade { get; set; } // 0-100 arası
        public string? LetterGrade { get; set; }
        public double? GradeCoefficient { get; set; } // 4.0 / 3.5 vs

        [NotMapped]
        public bool IsPlaceholder => CourseId == null && !string.IsNullOrEmpty(PlaceholderName);

        [NotMapped]
        public bool IsCountedInGPA => !string.IsNullOrEmpty(LetterGrade);
    }
}