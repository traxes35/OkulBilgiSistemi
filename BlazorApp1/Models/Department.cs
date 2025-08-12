using BlazorApp1.Data;

namespace BlazorApp1.Models
{
    public class Department
    {
        public int Id { get; set; } // int IDENTITY PK
        public string Name { get; set; } = null!; // nvarchar(200) NOT NULL

        public int FacultyId { get; set; } // int, NOT NULL
        public Faculty Faculty { get; set; } = null!;

        public virtual ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
        public virtual ICollection<Course> Courses { get; set; } = new List<Course>();
    }
}