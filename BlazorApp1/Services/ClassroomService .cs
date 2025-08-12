using BlazorApp1.Data;
using BlazorApp1.Models;
using BlazorApp1.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp1.Services
{
    public class ClassroomService : IClassroomService
    {
        private readonly ApplicationDbContext _context;

        public ClassroomService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Classroom>> GetAllAsync()
        {
            return await _context.Classrooms
                .Include(c => c.Department) // ✅ Bu satır eklendi
                .ToListAsync();
        }

        public async Task<Classroom?> GetByIdAsync(int id)
        {
            return await _context.Classrooms.FindAsync(id);
        }

        public async Task AddAsync(Classroom classroom)
        {
            _context.Classrooms.Add(classroom);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Classroom classroom)
        {
            _context.Classrooms.Update(classroom);
            await _context.SaveChangesAsync();
        }
        public async Task<Classroom?> FindAvailableClassroomAsync(List<TimeSlot> slots, List<Classroom> classrooms)
        {
            foreach (var cls in classrooms)
            {
                bool conflict = false;
                foreach (var slot in slots)
                {
                    bool isTaken = await _context.TeacherCourseTimes.AnyAsync(t =>
                        t.TimeSlotId == slot.Id && t.ClassroomId == cls.Id);

                    if (isTaken)
                    {
                        conflict = true;
                        break;
                    }
                }

                if (!conflict)
                    return cls;
            }

            return null;
        }
        public async Task DeleteAsync(int id)
        {
            var classroom = await _context.Classrooms.FindAsync(id);
            if (classroom != null)
            {
                _context.Classrooms.Remove(classroom);
                await _context.SaveChangesAsync();
            }
        }
        public async Task<List<Classroom>> GetByDepartmentAsync(int departmentId)
        {
            return await _context.Classrooms
                .Where(c => c.DepartmentId == departmentId)
                .ToListAsync();
        }
    }
}