using Application.Features.Schools;
using Domain.Entities;
using Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Schools;

public class SchoolService(ApplicationDbContext context) : ISchoolService
{
    private readonly ApplicationDbContext _context = context;

    public async Task<int> CreateAsync(School school, CancellationToken ct = default)
    {
        _context.Schools.Add(school);
        await _context.SaveChangesAsync(ct);
        return school.Id;
    }

    public async Task<int> DeleteAsync(School school, CancellationToken ct = default)
    {
        _context.Schools.Remove(school);
        await _context.SaveChangesAsync(ct);
        return school.Id;
    }

    public async Task<List<School>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.Schools.ToListAsync(ct);
    }

    public async Task<School?> GetByIdAsync(int schoolId, CancellationToken ct = default)
    {
        return await _context.Schools.FindAsync(schoolId, ct);
    }

    public async Task<School?> GetByNameAsync(string name, CancellationToken ct = default)
    {
        return await _context.Schools.FirstOrDefaultAsync(s => s.Name == name);
    }

    public async Task<int> UpdateAsync(School school, CancellationToken ct = default)
    {
        _context.Schools.Update(school);
        await _context.SaveChangesAsync();
        return school.Id;
    }
}
