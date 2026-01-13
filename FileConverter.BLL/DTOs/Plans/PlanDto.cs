using System;

namespace FileConverter.BLL.DTOs.Plans
{
    public class PlanDto
    {
        public Guid PlanId { get; set; }

        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        public decimal MonthlyPrice { get; set; }

        public int MaxConversionPerMonth { get; set; }

        public long MaxUploadBytes { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
