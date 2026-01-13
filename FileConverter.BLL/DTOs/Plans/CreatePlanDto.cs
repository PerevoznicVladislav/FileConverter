namespace FileConverter.BLL.DTOs.Plans
{
    public class CreatePlanDto
    {
        public string Name { get; set; } = null!;

        public decimal MonthlyPrice { get; set; }

        public int MaxConversionPerMonth { get; set; }

        public long MaxUploadBytes { get; set; }

        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
