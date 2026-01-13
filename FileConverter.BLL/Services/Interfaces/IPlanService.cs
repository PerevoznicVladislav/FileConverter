using FileConverter.BLL.DTOs.Plans;

namespace FileConverter.BLL.Services.Interfaces;

public interface IPlanService
{
    Task<IEnumerable<PlanDto>> GetAllAsync();
    Task<PlanDto?> GetByIdAsync(Guid id);

    Task<PlanDto> CreateAsync(CreatePlanDto createPlanDto);
    Task<PlanDto> UpdateAsync(EditPlanDto editPlanDto);

    Task<bool> DeleteAsync(Guid id);

    /// <summary>
    /// Verifică dacă planul poate fi șters (ex: nu are UserPlans active).
    /// </summary>
    Task<bool> CanDeleteAsync(Guid id);
}
