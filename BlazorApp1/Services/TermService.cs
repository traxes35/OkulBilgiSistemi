using BlazorApp1.Data;
using BlazorApp1.Models;
using BlazorApp1.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp1.Services
{
    public class TermService : ITermService
    {
        private readonly ApplicationDbContext _context;

        public TermService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Term>> GetAllTermsAsync()
        {
            return await _context.Terms.ToListAsync();
        }

        public async Task<Term?> GetActiveTermAsync()
        {
            return await _context.Terms.FirstOrDefaultAsync(t => t.IsActive);
        }

        public async Task<Term?> GetByIdAsync(int id)
        {
            return await _context.Terms.FindAsync(id);
        }

        public async Task AddTermAsync(Term term)
        {
            _context.Terms.Add(term);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateTermAsync(Term term)
        {
            Console.WriteLine($"Updating term ID: {term.Id}, IsActive: {term.IsActive}");
            _context.Terms.Update(term);
            await _context.SaveChangesAsync();
            Console.WriteLine("Update saved to database");
        }

        public async Task DeleteTermAsync(int id)
        {
            var term = await _context.Terms.FindAsync(id);
            if (term != null)
            {
                _context.Terms.Remove(term);
                await _context.SaveChangesAsync();
            }
        }
        public async Task<Term?> GetPreviousGuzTermAsync()
        {
            var activeTerm = await GetActiveTermAsync();
            if (activeTerm == null)
                return null;

            int prevYear = activeTerm.Semester == 1 ? activeTerm.Year - 1 : activeTerm.Year;
            int prevSemester = 1; // Güz dönemi

            return await _context.Terms
                .FirstOrDefaultAsync(t => t.Year == prevYear && t.Semester == prevSemester);
        }
    }
}