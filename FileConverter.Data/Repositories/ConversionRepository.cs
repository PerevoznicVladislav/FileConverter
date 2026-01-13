using FileConverter.Data.Models;
using FileConverter.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FileConverter.Data.Repositories;

public class ConversionRepository : IConversionRepository
{
    private readonly FileConverterDbContext _context;

    public ConversionRepository(FileConverterDbContext context)
    {
        _context = context;
    }

    public async Task<Conversion?> GetByIdAsync(Guid id)
    {
        return await _context.Conversions
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.ConversionId == id);
    }

    public async Task<IEnumerable<Conversion>> GetByUserIdAsync(Guid userId)
    {
        return await _context.Conversions
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<Conversion> AddAsync(Conversion conversion)
    {
        _context.Conversions.Add(conversion);
        await _context.SaveChangesAsync();
        return conversion;
    }

    public async Task<Conversion> UpdateAsync(Conversion conversion)
    {
        _context.Conversions.Update(conversion);
        await _context.SaveChangesAsync();
        return conversion;
    }

    public async Task<Conversion?> GetFirstConversionByUserIdAsync(Guid userId)
    {
        return await _context.Conversions
            .Where(c => c.UserId == userId)
            .OrderBy(c => c.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<int> GetConversionCountInPeriodAsync(Guid userId, DateTime startDate, DateTime endDate)
    {
        return await _context.Conversions
            .Where(c => c.UserId == userId && c.CreatedAt >= startDate && c.CreatedAt <= endDate)
            .CountAsync();
    }
}
