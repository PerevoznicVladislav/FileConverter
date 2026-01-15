using FileConverter.BLL.DTOs.Conversions;
using FileConverter.BLL.Services.Interfaces;
using FileConverter.Data.Models;
using FileConverter.Data.Repositories.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml;
using Text = DocumentFormat.OpenXml.Wordprocessing.Text;

namespace FileConverter.BLL.Services;

public class ConversionService : IConversionService
{
    private readonly IConversionRepository _conversionRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUserPlanRepository _userPlanRepository;
    private readonly IPlanRepository _planRepository;
    private readonly ILogger<ConversionService> _logger;

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
        IConfiguration configuration,
        ILogger<ConversionService> logger)
    {
        _conversionRepository = conversionRepository;
        _userRepository = userRepository;
        _userPlanRepository = userPlanRepository;
        _planRepository = planRepository;
        _logger = logger;

        var basePath = configuration["FileStorage:BasePath"]
            ?? Path.Combine(Directory.GetCurrentDirectory(), "Uploads");

        _uploadsPath = Path.Combine(basePath, "Input");
        _outputsPath = Path.Combine(basePath, "Output");

        Directory.CreateDirectory(_uploadsPath);
        Directory.CreateDirectory(_outputsPath);

        _logger.LogInformation(
            "ConversionService initialized. InputPath={InputPath}, OutputPath={OutputPath}",
            _uploadsPath, _outputsPath);
    }

    public async Task<ConversionResponseDto> ConvertPdfToWordAsync(
        Guid userId, Stream pdfStream, string fileName)
    {
        _logger.LogInformation(
            "Starting conversion. UserId={UserId}, FileName={FileName}",
            userId, fileName);

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("User not found. UserId={UserId}", userId);
            throw new InvalidOperationException("User not found");
        }

        // Validare fișier
        if (!fileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning(
                "Invalid file type. UserId={UserId}, FileName={FileName}",
                userId, fileName);

            throw new InvalidOperationException("File must be a PDF");
        }

        var fileSize = pdfStream.Length;
        if (fileSize == 0)
        {
            _logger.LogWarning(
                "Empty file uploaded. UserId={UserId}", userId);

            throw new InvalidOperationException("File is empty");
        }

        _logger.LogInformation(
            "File validated. Size={FileSize} bytes", fileSize);

        // Obține planul utilizatorului
        var userPlan = await _userPlanRepository.GetByUserIdAsync(userId);

        _logger.LogInformation(
            "User plan loaded. UserId={UserId}, HasPlan={HasPlan}",
            userId, userPlan != null);

        // Verifică limitele planului
        await ValidatePlanLimitsAsync(userId, userPlan, fileSize);

        // Salvează fișierul input
        var inputFileName = $"{Guid.NewGuid()}_{fileName}";
        var inputPath = Path.Combine(_uploadsPath, inputFileName);

        _logger.LogInformation(
            "Saving input file to {InputPath}", inputPath);

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

        _logger.LogInformation(
            "Conversion record created. ConversionId={ConversionId}",
            conversion.ConversionId);

        try
        {
            // Realizează conversia
            var outputFileName = $"{Guid.NewGuid()}.docx";
            var outputPath = Path.Combine(_outputsPath, outputFileName);

            _logger.LogInformation(
                "Starting file conversion. OutputPath={OutputPath}",
                outputPath);

            await ConvertPdfToWordFileAsync(inputPath, outputPath);

            conversion.OutputPath = outputPath;
            conversion.Status = "Completed";

            _logger.LogInformation(
                "Conversion completed successfully. ConversionId={ConversionId}",
                conversion.ConversionId);
        }
        catch (Exception ex)
        {
            conversion.Status = "Failed";
            await _conversionRepository.UpdateAsync(conversion);

            _logger.LogError(
                ex,
                "Conversion failed. ConversionId={ConversionId}",
                conversion.ConversionId);

            throw new InvalidOperationException(
                $"Conversion failed: {ex.Message}");
        }

        await _conversionRepository.UpdateAsync(conversion);

        return new ConversionResponseDto
        {
            ConversionId = conversion.ConversionId,
            Status = conversion.Status,
            Message = "Conversion completed successfully"
        };
    }

    public async Task<byte[]?> GetConvertedFileAsync(
        Guid userId, Guid conversionId)
    {
        _logger.LogInformation(
            "Download requested. ConversionId={ConversionId}",
            conversionId);

        var conversion = await _conversionRepository.GetByIdAsync(conversionId);
        if (conversion == null || conversion.UserId != userId)
        {
            _logger.LogWarning(
                "Unauthorized or missing conversion. ConversionId={ConversionId}",
                conversionId);

            return null;
        }

        if (string.IsNullOrEmpty(conversion.OutputPath)
            || !File.Exists(conversion.OutputPath))
        {
            _logger.LogWarning(
                "Output file missing. Path={OutputPath}",
                conversion.OutputPath);

            return null;
        }

        return await File.ReadAllBytesAsync(conversion.OutputPath);
    }

    private async Task ValidatePlanLimitsAsync(
        Guid userId, UserPlan? userPlan, long fileSize)
    {
        Plan? plan = null;

        if (userPlan != null)
        {
            plan = await _planRepository.GetByIdAsync(userPlan.PlanId);

            if (plan == null || !plan.IsActive)
            {
                _logger.LogWarning(
                    "Inactive or missing plan. UserId={UserId}",
                    userId);

                throw new InvalidOperationException(
                    "User plan is not active");
            }
        }

        // Verifică dimensiunea fișierului
        if (plan != null)
        {
            if (fileSize > plan.MaxUploadBytes)
            {
                throw new InvalidOperationException(
                    $"File size exceeds the maximum allowed size of " +
                    $"{plan.MaxUploadBytes / (1024 * 1024)}MB for plan {plan.Name}");
            }

            // Plan Premium: conversii nelimitate, doar verifică dimensiunea
            if (plan.Name.Equals("Premium",
                StringComparison.OrdinalIgnoreCase))
            {
                // Premium are conversii nelimitate, doar verifică dimensiunea (max 100MB)
                if (fileSize > PremiumMaxFileSize)
                    throw new InvalidOperationException(
                        "File size exceeds 100MB for Premium plan");

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
            var firstConversion =
                await _conversionRepository
                    .GetFirstConversionByUserIdAsync(userId);

            if (firstConversion == null)
            {
                // Prima conversie - permisă
                periodStart =
                    DateTime.UtcNow.AddDays(-FreePlanDaysPeriod);
            }
            else
            {
                // Verifică dacă au trecut 30 zile de la prima conversie
                var daysSinceFirstConversion =
                    (DateTime.UtcNow - firstConversion.CreatedAt).TotalDays;

                if (daysSinceFirstConversion >= FreePlanDaysPeriod)
                {
                    // Au trecut 30 zile, resetăm perioada
                    periodStart =
                        DateTime.UtcNow.AddDays(-FreePlanDaysPeriod);
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
            periodStart = new DateTime(
                DateTime.UtcNow.Year,
                DateTime.UtcNow.Month,
                1);
        }

        var conversionCount =
            await _conversionRepository
                .GetConversionCountInPeriodAsync(
                    userId, periodStart, periodEnd);

        if (conversionCount >= maxConversions)
        {
            var planName = plan?.Name ?? "Free";

            _logger.LogWarning(
                "Conversion limit reached. UserId={UserId}, Plan={PlanName}",
                userId, planName);

            throw new InvalidOperationException(
                $"You have reached the maximum number of conversions " +
                $"({maxConversions}) for {planName} plan. " +
                $"Please upgrade to Premium plan to continue.");
        }
    }

    private async Task ConvertPdfToWordFileAsync(
        string inputPath, string outputPath)
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
                new Run(new Text($"Converted from PDF: {pdfFileName}"))));

            body.AppendChild(new Paragraph(
                new Run(new Text("This is a DEMO conversion."))));

            body.AppendChild(new Paragraph(
                new Run(new Text(
                    "For real PDF → Word conversion, use a dedicated library."))));

            mainPart.Document.Save();
        });

        _logger.LogDebug(
            "DOCX file generated. Input={InputPath}, Output={OutputPath}",
            inputPath, outputPath);
    }
}
