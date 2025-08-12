namespace BlazorApp1.Models
{
    public class CourseGradeType
    {
        public int Id { get; set; }

        public int CourseId { get; set; }
        public int GradeTypeId { get; set; }

        public double Weight { get; set; }
        public bool IsLocked { get; set; } = false; // artık düzenlenemez
        public bool IsMakeupExam { get; set; } = false; // bu bir bütünleme mi
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual Course Course { get; set; } = null!;
        public virtual GradeType GradeType { get; set; } = null!;
        public virtual ICollection<StudentGrade> StudentGrades { get; set; } = new List<StudentGrade>();

    }
}
