using BlazorApp1.Data;
using BlazorApp1.Models;
using System.ComponentModel.DataAnnotations.Schema;

public class StudentGrade
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int CourseGradeTypeId { get; set; }

    public double Score { get; set; }
    public int VersionNumber { get; set; }
    public DateTime Date { get; set; }
    public bool IsDraft { get; set; }

    public virtual CourseGradeType CourseGradeType { get; set; } = null!;

    public virtual ApplicationUser User { get; set; } = null!;
}