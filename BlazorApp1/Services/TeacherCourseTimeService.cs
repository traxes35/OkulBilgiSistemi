using BlazorApp1.Data;
using BlazorApp1.Models;
using BlazorApp1.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq;

public class TeacherCourseTimeService : ITeacherCourseTimeService
{
    private readonly ApplicationDbContext _context;

    public TeacherCourseTimeService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<TeacherCourseTime>> GetAllAsync()
    {
        return await _context.TeacherCourseTimes
            .Include(x => x.Course)
            .Include(x => x.TimeSlot)
            .Include(x => x.Classroom)
            .ToListAsync();
    }

    public async Task<List<TeacherCourseTime>> GetByTeacherIdAsync(int teacherId)
    {
        return await _context.TeacherCourseTimes
            .Include(x => x.Course)
            .Include(x => x.TimeSlot)
            .Include(x => x.Classroom)
            .Where(x => x.UserId == teacherId)
            .ToListAsync();
    }

    public async Task<bool> AssignAsync(TeacherCourseTime assignment)
    {
        _context.TeacherCourseTimes.Add(assignment);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> RemoveAsync(int id)
    {
        var item = await _context.TeacherCourseTimes.FindAsync(id);
        if (item == null)
            return false;

        _context.TeacherCourseTimes.Remove(item);
        return await _context.SaveChangesAsync() > 0;
    }
    public async Task RemoveAllByTeacherIdAsync(int teacherId)
    {
        var records = _context.TeacherCourseTimes.Where(x => x.UserId == teacherId);
        _context.TeacherCourseTimes.RemoveRange(records);
        await _context.SaveChangesAsync();
    }
    public async Task<List<TeacherCourseTime>> GetByCourseIdsAsync(List<int> courseIds)
    {
        return await _context.TeacherCourseTimes
            .Where(tct => courseIds.Contains((int)tct.CourseId))
            .ToListAsync();
    }
    public async Task SyncTeacherAssignmentsAsync(int teacherId, List<TeacherCourseTime> newAssignments)
    {
        var existing = await _context.TeacherCourseTimes
            .Where(t => t.UserId == teacherId)
            .ToListAsync();

        var toRemove = existing.Where(e => !newAssignments.Any(n =>
            n.CourseId == e.CourseId && n.TimeSlotId == e.TimeSlotId)).ToList();

        var toAdd = newAssignments.Where(n => !existing.Any(e =>
            e.CourseId == n.CourseId && e.TimeSlotId == n.TimeSlotId)).ToList();

        _context.TeacherCourseTimes.RemoveRange(toRemove);
        _context.TeacherCourseTimes.AddRange(toAdd);

        await _context.SaveChangesAsync();
    }
}
