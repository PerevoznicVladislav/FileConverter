using System.Security.Claims;
using FileConverter.BLL.DTOs.Conversions;
using FileConverter.BLL.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FileConverter.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ConversionsController : ControllerBase
{
    private readonly IConversionService _conversionService;

    public ConversionsController(IConversionService conversionService)
    {
        _conversionService = conversionService;
    }

    /// <summary>
    /// Convertește un fișier PDF în format Word
    /// </summary>
    /// <param name="file">Fișierul PDF de convertit</param>
    /// <returns>Detalii despre conversie</returns>
    [HttpPost("convert")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ConversionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Convert(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "File is required" });

        if (!file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { message = "File must be a PDF" });

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized(new { message = "Invalid user" });

        try
        {
            using var stream = file.OpenReadStream();
            var result = await _conversionService.ConvertPdfToWordAsync(userId, stream, file.FileName);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"An error occurred: {ex.Message}" });
        }
    }

    /// <summary>
    /// Descarcă fișierul convertit
    /// </summary>
    /// <param name="id">ID-ul conversiei</param>
    /// <returns>Fișierul convertit pentru descărcare</returns>
    [HttpGet("{id}/download")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Download(Guid id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized(new { message = "Invalid user" });

        var fileBytes = await _conversionService.GetConvertedFileAsync(userId, id);
        if (fileBytes == null)
            return NotFound(new { message = "Conversion not found or file not available" });

        return File(fileBytes, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", $"converted_{id}.docx");
    }
}
