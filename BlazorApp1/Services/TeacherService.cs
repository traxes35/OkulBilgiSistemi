using BlazorApp1.Data;
using BlazorApp1.Models;
using BlazorApp1.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp1.Services
{
    public class TeacherService : ITeacherService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public TeacherService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<List<ApplicationUser>> GetAllTeachersAsync()
        {
            var allUsers = await _userManager.Users.ToListAsync();
            var teachers = new List<ApplicationUser>();

            foreach (var user in allUsers)
            {
                if (await _userManager.IsInRoleAsync(user, "Öğretmen"))
                {
                    teachers.Add(user);
                }
            }

            return teachers;
        }
    }
}
