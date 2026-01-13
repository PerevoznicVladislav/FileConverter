using FileConverter.BLL.DTOs.Plans;
using FileConverter.BLL.Services.Interfaces;
using FileConverter.Data.Models;
using FileConverter.Data.Repositories.Interfaces;

namespace FileConverter.BLL.Services;

public class PlanService : IPlanService
{
    private readonly IPlanRepository _planRepository;

    public PlanService(IPlanRepository planRepository)
    {
        _planRepository = planRepository;
    }

    public async Task<IEnumerable<PlanDto>> GetAllAsync()
    {
        var plans = await _planRepository.GetAllAsync();
        return plans.Select(MapToDto);
    }

    public async Task<PlanDto?> GetByIdAsync(Guid id)
    {
        var plan = await _planRepository.GetByIdAsync(id);
        if (plan == null)
            return null;

        return MapToDto(plan);
    }

    public async Task<PlanDto> CreateAsync(CreatePlanDto createPlanDto)
    {
        // Name este unic în DB (UQ_Plans_Name)
        if (await _planRepository.ExistsByNameAsync(createPlanDto.Name))
        {
            throw new InvalidOperationException(
                $"Planul '{createPlanDto.Name}' există deja.");
        }

        var plan = new Plan
        {
            // PlanId e generat de DB (IDENTITY), nu îl setăm manual
            Name = createPlanDto.Name.Trim(),
            MonthlyPrice = createPlanDto.MonthlyPrice,
            MaxConversionPerMonth = createPlanDto.MaxConversionPerMonth,
            MaxUploadBytes = createPlanDto.MaxUploadBytes,
            Description = string.IsNullOrWhiteSpace(createPlanDto.Description)
                ? null
                : createPlanDto.Description.Trim(),
            IsActive = createPlanDto.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        var createdPlan = await _planRepository.AddAsync(plan);
        return MapToDto(createdPlan);
    }

    public async Task<PlanDto> UpdateAsync(EditPlanDto editPlanDto)
    {
        var existingPlan = await _planRepository.GetByIdAsync(editPlanDto.PlanId);
        if (existingPlan == null)
        {
            throw new InvalidOperationException(
                $"Planul cu ID '{editPlanDto.PlanId}' nu a fost găsit.");
        }

        // verificăm unicitatea numelui excluzând planul curent
        if (await _planRepository.ExistsByNameAsync(editPlanDto.Name, editPlanDto.PlanId))
        {
            throw new InvalidOperationException(
                $"Planul cu numele '{editPlanDto.Name}' există deja.");
        }

        existingPlan.Name = editPlanDto.Name.Trim();
        existingPlan.MonthlyPrice = editPlanDto.MonthlyPrice;
        existingPlan.MaxConversionPerMonth = editPlanDto.MaxConversionPerMonth;
        existingPlan.MaxUploadBytes = editPlanDto.MaxUploadBytes;
        existingPlan.Description = string.IsNullOrWhiteSpace(editPlanDto.Description)
            ? null
            : editPlanDto.Description.Trim();
        existingPlan.IsActive = editPlanDto.IsActive;

        var updatedPlan = await _planRepository.UpdateAsync(existingPlan);
        return MapToDto(updatedPlan);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        if (!await CanDeleteAsync(id))
        {
            throw new InvalidOperationException(
                "Planul nu poate fi șters deoarece este utilizat de utilizatori (UserPlans).");
        }

        return await _planRepository.DeleteAsync(id);
    }

    public async Task<bool> CanDeleteAsync(Guid id)
    {
        // dacă are legături active/asociate în UserPlans -> nu permitem ștergerea
        return !await _planRepository.HasActiveUserPlansAsync(id);
    }

    private static PlanDto MapToDto(Plan plan)
    {
        return new PlanDto
        {
            PlanId = plan.PlanId,
            Name = plan.Name,
            Description = plan.Description,
            MonthlyPrice = plan.MonthlyPrice,
            MaxConversionPerMonth = plan.MaxConversionPerMonth,
            MaxUploadBytes = plan.MaxUploadBytes,
            IsActive = plan.IsActive,
            CreatedAt = plan.CreatedAt
        };
    }
}
