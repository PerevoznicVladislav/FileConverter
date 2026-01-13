using FileConverter.BLL.DTOs.Conversions;
using FileConverter.BLL.Services.Interfaces;
using FileConverter.Data.Models;
using FileConverter.Data.Repositories.Interfaces;
using Microsoft.Extensions.Configuration;
using static System.Net.Mime.MediaTypeNames;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Text = DocumentFormat.OpenXml.Wordprocessing.Text;
using DocumentFormat.OpenXml;

namespace FileConverter.BLL.Services;

public class ConversionService : IConversionService
{
    private readonly IConversionRepository _conversionRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUserPlanRepository _userPlanRepository;
    private readonly IPlanRepository _planRepository;
    private readonly string _uploadsPath;
    private readonly string _outputsPath;

    private const int FreePlanMaxConversions = 5;
    private const int FreePlanDaysPeriod = 30;
    private const long PremiumMaxFileSize = 100 * 1024 * 1024; // 100MB

    public ConversionService(
        IConversionRepository conversionRepository,
        IUserRepository userRepository,
        IUserPlanRepository userPlanRepository,
        IPlanRepository planRepository,
        IConfiguration configuration)
    {
        _conversionRepository = conversionRepository;
        _userRepository = userRepository;
        _userPlanRepository = userPlanRepository;
        _planRepository = planRepository;
        
        var basePath = configuration["FileStorage:BasePath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
        _uploadsPath = Path.Combine(basePath, "Input");
        _outputsPath = Path.Combine(basePath, "Output");
        
        Directory.CreateDirectory(_uploadsPath);
        Directory.CreateDirectory(_outputsPath);
    }

    public async Task<ConversionResponseDto> ConvertPdfToWordAsync(Guid userId, Stream pdfStream, string fileName)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new InvalidOperationException("User not found");

        // Validare fișier
        if (!fileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("File must be a PDF");

        var fileSize = pdfStream.Length;
        if (fileSize == 0)
            throw new InvalidOperationException("File is empty");

        // Obține planul utilizatorului
        var userPlan = await _userPlanRepository.GetByUserIdAsync(userId);
        
        // Verifică limitele planului
        await ValidatePlanLimitsAsync(userId, userPlan, fileSize);

        // Salvează fișierul input
        var inputFileName = $"{Guid.NewGuid()}_{fileName}";
        var inputPath = Path.Combine(_uploadsPath, inputFileName);
        
        using (var fileStream = new FileStream(inputPath, FileMode.Create))
        {
            await pdfStream.CopyToAsync(fileStream);
        }

        // Creează conversia
        var conversion = new Conversion
        {
            ConversionId = Guid.NewGuid(),
            UserId = userId,
            FromType = "PDF",
            ToType = "WORD",
            InputFileName = fileName,
            InputPath = inputPath,
            Status = "Processing",
            CreatedAt = DateTime.UtcNow
        };

        conversion = await _conversionRepository.AddAsync(conversion);

        try
        {
            // Realizează conversia
            var outputFileName = $"{Guid.NewGuid()}.docx";
            var outputPath = Path.Combine(_outputsPath, outputFileName);
            
            await ConvertPdfToWordFileAsync(inputPath, outputPath);

            conversion.OutputPath = outputPath;
            conversion.Status = "Completed";
        }
        catch (Exception ex)
        {
            conversion.Status = "Failed";
            await _conversionRepository.UpdateAsync(conversion);
            throw new InvalidOperationException($"Conversion failed: {ex.Message}");
        }

        await _conversionRepository.UpdateAsync(conversion);

        return new ConversionResponseDto
        {
            ConversionId = conversion.ConversionId,
            Status = conversion.Status,
            Message = "Conversion completed successfully"
        };
    }

    public async Task<byte[]?> GetConvertedFileAsync(Guid userId, Guid conversionId)
    {
        var conversion = await _conversionRepository.GetByIdAsync(conversionId);
        if (conversion == null || conversion.UserId != userId)
            return null;

        if (string.IsNullOrEmpty(conversion.OutputPath) || !File.Exists(conversion.OutputPath))
            return null;

        return await File.ReadAllBytesAsync(conversion.OutputPath);
    }

    private async Task ValidatePlanLimitsAsync(Guid userId, UserPlan? userPlan, long fileSize)
    {
        Plan? plan = null;
        
        if (userPlan != null)
        {
            plan = await _planRepository.GetByIdAsync(userPlan.PlanId);
            if (plan == null || !plan.IsActive)
                throw new InvalidOperationException("User plan is not active");
        }

        // Verifică dimensiunea fișierului
        if (plan != null)
        {
            if (fileSize > plan.MaxUploadBytes)
                throw new InvalidOperationException($"File size exceeds the maximum allowed size of {plan.MaxUploadBytes / (1024 * 1024)}MB for plan {plan.Name}");

            // Plan Premium: conversii nelimitate, doar verifică dimensiunea
            if (plan.Name.Equals("Premium", StringComparison.OrdinalIgnoreCase))
            {
                // Premium are conversii nelimitate, doar verifică dimensiunea (max 100MB)
                if (fileSize > PremiumMaxFileSize)
                    throw new InvalidOperationException($"File size exceeds the maximum allowed size of 100MB for Premium plan");
                return;
            }
        }

        // Plan Free sau Basic: verifică numărul de conversii
        int maxConversions;
        DateTime periodStart;
        DateTime periodEnd = DateTime.UtcNow;

        if (plan == null) // Free plan
        {
            maxConversions = FreePlanMaxConversions;
            
            // Pentru planul Free: verifică de la prima conversie
            var firstConversion = await _conversionRepository.GetFirstConversionByUserIdAsync(userId);
            if (firstConversion == null)
            {
                // Prima conversie - permisă
                periodStart = DateTime.UtcNow.AddDays(-FreePlanDaysPeriod);
            }
            else
            {
                // Verifică dacă au trecut 30 zile de la prima conversie
                var daysSinceFirstConversion = (DateTime.UtcNow - firstConversion.CreatedAt).TotalDays;
                if (daysSinceFirstConversion >= FreePlanDaysPeriod)
                {
                    // Au trecut 30 zile, resetăm perioada
                    periodStart = DateTime.UtcNow.AddDays(-FreePlanDaysPeriod);
                }
                else
                {
                    // Perioada încă activă - verifică de la prima conversie
                    periodStart = firstConversion.CreatedAt;
                }
            }
        }
        else // Basic plan
        {
            maxConversions = plan.MaxConversionPerMonth;
            
            // Pentru Basic: verifică conversiile din luna curentă
            periodStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        }

        var conversionCount = await _conversionRepository.GetConversionCountInPeriodAsync(userId, periodStart, periodEnd);
        
        if (conversionCount >= maxConversions)
        {
            var planName = plan?.Name ?? "Free";
            throw new InvalidOperationException($"You have reached the maximum number of conversions ({maxConversions}) for {planName} plan. Please upgrade to Premium plan to continue.");
        }
    }

    private async Task ConvertPdfToWordFileAsync(string inputPath, string outputPath)
    {
        await Task.Run(() =>
        {
            var pdfFileName = Path.GetFileName(inputPath);

            using var doc = WordprocessingDocument.Create(
                outputPath,
                WordprocessingDocumentType.Document);

            var mainPart = doc.AddMainDocumentPart();
            mainPart.Document = new Document();
            var body = mainPart.Document.AppendChild(new Body());

            body.AppendChild(new Paragraph(
                new Run(new Text($"Converted from PDF: {pdfFileName}"))
            ));

            body.AppendChild(new Paragraph(
                new Run(new Text("This is a DEMO conversion."))
            ));

            body.AppendChild(new Paragraph(
                new Run(new Text("For real PDF → Word conversion, use a dedicated library."))
            ));

            mainPart.Document.Save();
        });
    }
}
