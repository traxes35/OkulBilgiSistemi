using BlazorApp1.Models;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorApp1.Data;

// Add profile data for application users by adding properties to the ApplicationUser class
public class ApplicationUser : IdentityUser<int>
{
    [Required, StringLength(50)]
    public string Isim { get; set; }

    [Required, StringLength(50)]
    public string Soyisim { get; set; }

    // Admin i�in
    [StringLength(100)]
    public string? Departman { get; set; }


    // ��retmen i�in
    public int? MaxSecmeliDers { get; set; }

    // ��renci i�in
    public int? KrediLimiti { get; set; }
    public int? SinifSeviyesi { get; set; }  // NULL olabilir ama �nerilmez
    public int? DepartmentId { get; set; }
    public Department? Department { get; set; }
    public ICollection<Simulation> Simulations { get; set; } = new List<Simulation>();

    public ICollection<StudentGrade> StudentGrades { get; set; } = new List<StudentGrade>();

}


