namespace BlazorApp1.Models
{
    public class Faculty
    {
        public int Id { get; set; } // int IDENTITY PK
        public string Name { get; set; } = null!; // nvarchar(200) NOT NULL

        public virtual ICollection<Department> Departments { get; set; } = new List<Department>();
    }
}
