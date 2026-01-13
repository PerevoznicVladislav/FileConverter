using FileConverter.Data.Models;

namespace FileConverter.Data.Repositories.Interfaces;

public interface IPlanRepository
{
    Task<IEnumerable<Plan>> GetAllAsync();
    Task<Plan?> GetByIdAsync(Guid id);

    Task<Plan> AddAsync(Plan plan);
    Task<Plan> UpdateAsync(Plan plan);
    Task<bool> DeleteAsync(Guid id);

    Task<bool> PlanExistsAsync(Guid id);

    // Name este unic în DB (UQ_Plans_Name). excludeId e util la editare.
    Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null);

    // Verifică dacă planul este folosit de utilizatori (UserPlans active).
    Task<bool> HasActiveUserPlansAsync(Guid planId);
}
