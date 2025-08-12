using BlazorApp1.Data;

public class UserWithRoles
{
    public ApplicationUser User { get; set; }
    public List<string> Roles { get; set; }
}