using System;
using System.Collections.Generic;

namespace BlazorApp1.Models;

public partial class SimulationCourse
{
    public int Id { get; set; }

    public int CourseCode { get; set; }

    public bool IsFree { get; set; }

    public string? CourseName { get; set; }

    public string? TermInfo { get; set; }

    public int Credit { get; set; }

    public string? LetterGrade { get; set; }

    public double GradeCoefficient { get; set; }
}
