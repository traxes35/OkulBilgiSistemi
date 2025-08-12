using System;
using System.Collections.Generic;

namespace BlazorApp1.Models;

public class LetterGradeRange
{
    public int Id { get; set; } // ✅ eklendi

    public int CourseId { get; set; }
    public string Letter { get; set; } = null!;
    public int LowerBound { get; set; }
    public int UpperBound { get; set; }
    public double Coefficient { get; set; }

    public virtual Course Course { get; set; } = null!;
}
