using System;
using System.Collections.Generic;

namespace BlazorApp1.Models;

public partial class TimeSlot
{
    public int Id { get; set; }

    public string? Day { get; set; }

    public TimeOnly? StartTime { get; set; }

    public TimeOnly? EndTime { get; set; }

    public virtual ICollection<TeacherCourseTime> TeacherCourseTimes { get; set; } = new List<TeacherCourseTime>();
}
