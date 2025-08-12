using System;
using System.Collections.Generic;

namespace BlazorApp1.Models;

public class TeacherCourse
{
    public int Id { get; set; } // ✅ eklendi

    public int UserId { get; set; }
    public int CourseId { get; set; }

    public bool? IsResponsibilityApproved { get; set; }
    public bool IsActive { get; set; }

    public virtual Course Course { get; set; } = null!;
}