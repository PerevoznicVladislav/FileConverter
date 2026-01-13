using FileConverter.BLL.Services.Interfaces;
using FileConverter.BLL.DTOs; // ajustează namespace-urile DTO-urilor tale
using Microsoft.AspNetCore.Mvc;
using FileConverter.BLL.DTOs.Plans;

namespace FileConverter.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlansController : ControllerBase
    {
        private readonly IPlanService _planService;

        public PlansController(IPlanService planService)
        {
            _planService = planService;
        }

        // GET: /api/plans
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var plans = await _planService.GetAllAsync();
            return Ok(plans);
        }

        // GET: /api/plans/{id}
        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var plan = await _planService.GetByIdAsync(id);
            if (plan == null)
                return NotFound(new { message = "Plan not found." });

            return Ok(plan);
        }

        // POST: /api/plans
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreatePlanDto dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            try
            {
                // Ideal: CreateAsync returnează planul creat (sau ID-ul).
                // Dacă la tine CreateAsync întoarce void, vezi nota de mai jos.
                var created = await _planService.CreateAsync(dto);

                // Dacă created are PlanId:
                return CreatedAtAction(nameof(GetById), new { id = created.PlanId }, created);
            }
            catch (InvalidOperationException ex)
            {
                // ex: nume duplicat, reguli business etc.
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT: /api/plans/{id}
        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(Guid id, [FromBody] EditPlanDto dto)
        {
            if (dto == null || id != dto.PlanId)
                return BadRequest(new { message = "Route id must match dto.PlanId." });

            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var existing = await _planService.GetByIdAsync(id);
            if (existing == null)
                return NotFound(new { message = "Plan not found." });

            try
            {
                await _planService.UpdateAsync(dto);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // DELETE: /api/plans/{id}
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var plan = await _planService.GetByIdAsync(id);
            if (plan == null)
                return NotFound(new { message = "Plan not found." });

            // echivalent ViewBag.CanDelete
            var canDelete = await _planService.CanDeleteAsync(id);
            if (!canDelete)
                return BadRequest(new { message = "Plan cannot be deleted because it is in use." });

            try
            {
                await _planService.DeleteAsync(id);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
