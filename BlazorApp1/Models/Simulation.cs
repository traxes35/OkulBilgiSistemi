using BlazorApp1.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorApp1.Models;

public class Simulation
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }

    public string? Goal { get; set; }

    public DateTime CreatedAt { get; set; }

    // 🔗 Navigation Property
    public ApplicationUser Student { get; set; }

}
