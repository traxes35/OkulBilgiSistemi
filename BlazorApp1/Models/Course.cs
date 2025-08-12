using System;
using System.Collections.Generic;

namespace BlazorApp1.Models;

public partial class Course
{
    public int Id { get; set; }

    public int CourseCode { get; set; }

    public string? CourseName { get; set; }

    public int Credit { get; set; }

    public int Quota { get; set; }

    public bool IsMandatory { get; set; }

    public int TermId { get; set; }

    public int ClassLevel { get; set; }
    public int DepartmentId { get; set; }
    public Department Department { get; set; } = null!;

    public int WeeklySlotCount { get; set; }


    public virtual ICollection<LetterGradeRange> LetterGradeRanges { get; set; } = new List<LetterGradeRange>();

    public virtual ICollection<StudentCourse> StudentCourses { get; set; } = new List<StudentCourse>();


    public virtual ICollection<TeacherCourseTime> TeacherCourseTimes { get; set; } = new List<TeacherCourseTime>();

    public virtual ICollection<TeacherCourse> TeacherCourses { get; set; } = new List<TeacherCourse>();
    public virtual ICollection<CourseGradeType> CourseGradeTypes { get; set; } = new List<CourseGradeType>();
    public virtual Term Term { get; set; } = null!;
}
