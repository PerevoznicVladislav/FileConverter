using FileConverter.Data.Models;

namespace FileConverter.Data.Repositories.Interfaces;

public interface IConversionRepository
{
    Task<Conversion?> GetByIdAsync(Guid id);
    Task<IEnumerable<Conversion>> GetByUserIdAsync(Guid userId);
    Task<Conversion> AddAsync(Conversion conversion);
    Task<Conversion> UpdateAsync(Conversion conversion);
    Task<Conversion?> GetFirstConversionByUserIdAsync(Guid userId);
    Task<int> GetConversionCountInPeriodAsync(Guid userId, DateTime startDate, DateTime endDate);
}
