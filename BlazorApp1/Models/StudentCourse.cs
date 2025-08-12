using System;
using System.Collections.Generic;

namespace BlazorApp1.Models;

public class StudentCourse
{
    public int Id { get; set; } // ✅ tekil PK

    public int UserId { get; set; }
    public int CourseId { get; set; }

    public int? Grade { get; set; }
    public string? LetterGrade { get; set; }

    public bool HasScheduleConflict { get; set; }
    public bool IsQuotaFull { get; set; }
    public bool IsActive { get; set; }
    public int TermId { get; set; } 

    public virtual Term Term { get; set; }
    public virtual Course Course { get; set; } = null!;
}
