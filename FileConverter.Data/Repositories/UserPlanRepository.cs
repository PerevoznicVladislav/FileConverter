using FileConverter.Data.Models;
using FileConverter.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FileConverter.Data.Repositories;

public class UserPlanRepository : IUserPlanRepository
{
    private readonly FileConverterDbContext _context;

    public UserPlanRepository(FileConverterDbContext context)
    {
        _context = context;
    }

    public async Task<UserPlan?> GetByUserIdAsync(Guid userId)
    {
        return await _context.UserPlans
            .Include(up => up.Plan)
            .FirstOrDefaultAsync(up => up.UserId == userId);
    }

    public async Task<UserPlan> AddAsync(UserPlan userPlan)
    {
        _context.UserPlans.Add(userPlan);
        await _context.SaveChangesAsync();
        return userPlan;
    }

    public async Task<UserPlan> UpdateAsync(UserPlan userPlan)
    {
        _context.UserPlans.Update(userPlan);
        await _context.SaveChangesAsync();
        return userPlan;
    }

    public async Task<bool> DeleteByUserIdAsync(Guid userId)
    {
        var userPlan = await GetByUserIdAsync(userId);
        if (userPlan == null)
            return false;

        _context.UserPlans.Remove(userPlan);
        await _context.SaveChangesAsync();
        return true;
    }
}
