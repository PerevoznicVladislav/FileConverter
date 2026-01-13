namespace FileConverter.BLL.DTOs.Conversions;

public class ConversionResponseDto
{
    public Guid ConversionId { get; set; }
    public string Status { get; set; } = null!;
    public string Message { get; set; } = null!;
}
