using BlazorApp1.Models;

public class TeacherCourseTime
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public int? CourseId { get; set; }
    public int TimeSlotId { get; set; }
    public int ClassroomId { get; set; } // ✅ yeni alan

    public virtual Course? Course { get; set; }
    public virtual TimeSlot TimeSlot { get; set; } = null!;
    public virtual Classroom Classroom { get; set; } = null!;
}