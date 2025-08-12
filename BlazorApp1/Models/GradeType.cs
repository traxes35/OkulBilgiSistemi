using System;
using System.Collections.Generic;

namespace BlazorApp1.Models;

public class GradeType
{
    public int Id { get; set; }
    public string? GradeName { get; set; }
    public bool IsMandatory { get; set; } // 🔹 Vize/Final gibi zorunlu sınavlar için
    public bool IsUniquePerCourse { get; set; } // 🔹 Bu türden her ders için yalnızca 1 tane olabilir (örn. Final)

    public virtual ICollection<CourseGradeType> CourseGradeTypes { get; set; } = new List<CourseGradeType>();
    public virtual ICollection<StudentGrade> StudentGrades { get; set; } = new List<StudentGrade>();
}