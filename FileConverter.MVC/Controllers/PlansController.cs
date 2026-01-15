using FileConverter.BLL.DTOs.Plans;
using FileConverter.BLL.Services.Interfaces;
using FileConverter.MVC.Models.Plans;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FileConverter.MVC.Controllers
{
    [Authorize(Roles = "Admin")]
    public class PlansController : Controller
    {
        private readonly IPlanService _planService;

        public PlansController(IPlanService planService)
        {
            _planService = planService;
        }

        // GET: Plans
        public async Task<IActionResult> Index()
        {
            var plans = await _planService.GetAllAsync();

            var viewModels = plans.Select(p => new PlanModel
            {
                PlanId = p.PlanId,
                Name = p.Name,
                Description = p.Description,
                MonthlyPrice = p.MonthlyPrice,
                MaxConversionPerMonth = p.MaxConversionPerMonth,
                MaxUploadBytes = p.MaxUploadBytes,
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt
            });

            return View(viewModels);
        }


        // GET: Plans/Create
        public IActionResult Create()
        {
            return View(new CreatePlanModel());
        }

        // POST: Plans/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreatePlanModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var dto = new CreatePlanDto
                {
                    Name = model.Name,
                    Description = model.Description,
                    MonthlyPrice = model.MonthlyPrice,
                    MaxConversionPerMonth = model.MaxConversionPerMonth,
                    MaxUploadBytes = model.MaxUploadBytes,
                    IsActive = model.IsActive
                };

                await _planService.CreateAsync(dto);
                TempData["SuccessMessage"] = "Planul a fost creat cu succes!";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
        }

        // GET: Plans/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
                return NotFound();

            var plan = await _planService.GetByIdAsync(id.Value);
            if (plan == null)
                return NotFound();

            var model = new EditPlanModel
            {
                PlanId = plan.PlanId,
                Name = plan.Name,
                Description = plan.Description,
                MonthlyPrice = plan.MonthlyPrice,
                MaxConversionPerMonth = plan.MaxConversionPerMonth,
                MaxUploadBytes = plan.MaxUploadBytes,
                IsActive = plan.IsActive
            };

            return View(model);
        }

        // POST: Plans/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, EditPlanModel model)
        {
            if (id != model.PlanId)
                return NotFound();

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var dto = new EditPlanDto
                {
                    PlanId = model.PlanId,
                    Name = model.Name,
                    Description = model.Description,
                    MonthlyPrice = model.MonthlyPrice,
                    MaxConversionPerMonth = model.MaxConversionPerMonth,
                    MaxUploadBytes = model.MaxUploadBytes,
                    IsActive = model.IsActive
                };

                await _planService.UpdateAsync(dto);
                TempData["SuccessMessage"] = "Planul a fost actualizat cu succes!";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
        }

        // GET: Plans/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
                return NotFound();

            var plan = await _planService.GetByIdAsync(id.Value);
            if (plan == null)
                return NotFound();

            ViewBag.CanDelete = await _planService.CanDeleteAsync(id.Value);

            var viewModel = new PlanModel
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

            return View(viewModel);
        }

		// POST: Plans/DeleteConfirmed/5
		[HttpPost, ActionName("DeleteConfirmed")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(Guid planId)
		{
			try
			{
				await _planService.DeleteAsync(planId);
				TempData["SuccessMessage"] = "Planul a fost șters cu succes!";
			}
			catch (InvalidOperationException ex)
			{
				TempData["ErrorMessage"] = ex.Message;
			}

			return RedirectToAction(nameof(Index));
		}

	}
}
