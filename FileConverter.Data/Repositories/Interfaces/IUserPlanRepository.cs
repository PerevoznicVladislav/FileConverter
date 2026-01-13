using FileConverter.Data.Models;

namespace FileConverter.Data.Repositories.Interfaces;

public interface IUserPlanRepository
{
    Task<UserPlan?> GetByUserIdAsync(Guid userId);
    Task<UserPlan> AddAsync(UserPlan userPlan);
    Task<UserPlan> UpdateAsync(UserPlan userPlan);
    Task<bool> DeleteByUserIdAsync(Guid userId);
}
