using FileConverter.Data.Models;
using FileConverter.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FileConverter.Data.Repositories;

public class MonthlyUsageRepository : IMonthlyUsageRepository
{
    private readonly FileConverterDbContext _context;

    public MonthlyUsageRepository(FileConverterDbContext context)
    {
        _context = context;
    }

    public async Task<MonthlyUsage?> GetByUserAndPeriodAsync(Guid userId, int year, int month)
    {
        return await _context.MonthlyUsages
            .FirstOrDefaultAsync(mu => mu.UserId == userId && mu.Year == year && mu.Month == month);
    }

    public async Task<MonthlyUsage> AddAsync(MonthlyUsage monthlyUsage)
    {
        _context.MonthlyUsages.Add(monthlyUsage);
        await _context.SaveChangesAsync();
        return monthlyUsage;
    }

    public async Task<MonthlyUsage> UpdateAsync(MonthlyUsage monthlyUsage)
    {
        _context.MonthlyUsages.Update(monthlyUsage);
        await _context.SaveChangesAsync();
        return monthlyUsage;
    }

    public async Task<MonthlyUsage> GetOrCreateAsync(Guid userId, int year, int month)
    {
        var monthlyUsage = await GetByUserAndPeriodAsync(userId, year, month);
        if (monthlyUsage != null)
            return monthlyUsage;

        monthlyUsage = new MonthlyUsage
        {
            MonthlyUsageId = Guid.NewGuid(),
            UserId = userId,
            Year = year,
            Month = month,
            ConversionsUsed = 0
        };

        return await AddAsync(monthlyUsage);
    }
}
