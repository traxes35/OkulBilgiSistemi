using System;
using System.Collections.Generic;

namespace BlazorApp1.Models;

public partial class Term
{
    public int Id { get; set; }

    public int Year { get; set; }

    public int Semester { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<Course> Courses { get; set; } = new List<Course>();
}
