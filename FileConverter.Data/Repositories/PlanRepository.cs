using FileConverter.Data.Models;
using FileConverter.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FileConverter.Data.Repositories;

public class PlanRepository : IPlanRepository
{
    private readonly FileConverterDbContext _context;

    public PlanRepository(FileConverterDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Plan>> GetAllAsync()
    {
        return await _context.Plans
            .OrderBy(p => p.MonthlyPrice)
            .ThenBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<Plan?> GetByIdAsync(Guid id)
    {
        return await _context.Plans.FindAsync(id);
    }

    public async Task<Plan> AddAsync(Plan plan)
    {
        _context.Plans.Add(plan);
        await _context.SaveChangesAsync();
        return plan;
    }

    public async Task<Plan> UpdateAsync(Plan plan)
    {
        _context.Plans.Update(plan);
        await _context.SaveChangesAsync();
        return plan;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var plan = await _context.Plans.FindAsync(id);
        if (plan == null)
            return false;

        _context.Plans.Remove(plan);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> PlanExistsAsync(Guid id)
    {
        return await _context.Plans.AnyAsync(p => p.PlanId == id);
    }

    public async Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null)
    {
        var query = _context.Plans.Where(p => p.Name == name);

        if (excludeId.HasValue)
        {
            query = query.Where(p => p.PlanId != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task<bool> HasActiveUserPlansAsync(Guid planId)
    {
        // Variante posibile, în funcție de cum arată entitatea UserPlan în proiectul tău:

        // VARIANTA A (dacă ai un bool IsActive în UserPlan)
        // return await _context.UserPlans.AnyAsync(up => up.PlanId == planId && up.IsActive);

        // VARIANTA B (dacă ai EndDate / ExpirationDate)
        // return await _context.UserPlans.AnyAsync(up => up.PlanId == planId && up.EndDate == null);

        // VARIANTA C (fallback - verifică doar dacă există legături)
        return await _context.UserPlans.AnyAsync(up => up.PlanId == planId);
    }
}
