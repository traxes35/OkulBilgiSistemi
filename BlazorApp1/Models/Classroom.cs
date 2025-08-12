using System.ComponentModel.DataAnnotations;

namespace BlazorApp1.Models
{
    public class Classroom
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = null!;

        public int Capacity { get; set; }

        public bool IsActive { get; set; } = true;
        public int? DepartmentId { get; set; }
        public Department? Department { get; set; }
        public virtual ICollection<TeacherCourseTime> TeacherCourseTimes { get; set; } = new List<TeacherCourseTime>();
    }
}
