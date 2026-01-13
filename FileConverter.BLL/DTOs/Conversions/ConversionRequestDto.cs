namespace FileConverter.BLL.DTOs.Conversions;

using Microsoft.AspNetCore.Http;
public class ConversionRequestDto
{
    public IFormFile File { get; set; } = null!;
}
