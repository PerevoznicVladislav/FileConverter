using FileConverter.BLL.DTOs.Conversions;

namespace FileConverter.BLL.Services.Interfaces;

public interface IConversionService
{
    Task<ConversionResponseDto> ConvertPdfToWordAsync(Guid userId, Stream pdfStream, string fileName);
    Task<byte[]?> GetConvertedFileAsync(Guid userId, Guid conversionId);
}
