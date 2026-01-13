using FileConverter.Data.Models;

namespace FileConverter.Data.Repositories.Interfaces;

public interface IMonthlyUsageRepository
{
    Task<MonthlyUsage?> GetByUserAndPeriodAsync(Guid userId, int year, int month);
    Task<MonthlyUsage> AddAsync(MonthlyUsage monthlyUsage);
    Task<MonthlyUsage> UpdateAsync(MonthlyUsage monthlyUsage);
    Task<MonthlyUsage> GetOrCreateAsync(Guid userId, int year, int month);
}
