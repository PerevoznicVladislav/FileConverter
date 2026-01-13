using System;
using System.ComponentModel.DataAnnotations;

namespace FileConverter.MVC.Models.Plans
{
    public class PlanModel
    {
        public Guid PlanId { get; set; }

        [Display(Name = "Denumire")]
        public string Name { get; set; } = null!;

        [Display(Name = "Descriere")]
        public string? Description { get; set; }

        [Display(Name = "Preț lunar")]
        [DataType(DataType.Currency)]
        public decimal MonthlyPrice { get; set; }

        [Display(Name = "Max conversii / lună")]
        public int MaxConversionPerMonth { get; set; }

        [Display(Name = "Max upload (bytes)")]
        public long MaxUploadBytes { get; set; }

        [Display(Name = "Activ")]
        public bool IsActive { get; set; }

        [Display(Name = "Creat la")]
        public DateTime CreatedAt { get; set; }
    }
}
