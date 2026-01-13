using System.ComponentModel.DataAnnotations;

namespace FileConverter.MVC.Models.Plans
{
    public class CreatePlanModel
    {
        [Required(ErrorMessage = "Denumirea planului este obligatorie.")]
        [StringLength(50, ErrorMessage = "Denumirea nu poate depăși 50 caractere.")]
        [Display(Name = "Denumire")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Prețul lunar este obligatoriu.")]
        [Range(0, 99999999.99, ErrorMessage = "Prețul lunar trebuie să fie un număr pozitiv.")]
        [DataType(DataType.Currency)]
        [Display(Name = "Preț lunar")]
        public decimal MonthlyPrice { get; set; }

        [Required(ErrorMessage = "Numărul maxim de conversii pe lună este obligatoriu.")]
        [Range(0, int.MaxValue, ErrorMessage = "Valoarea trebuie să fie 0 sau mai mare.")]
        [Display(Name = "Max conversii / lună")]
        public int MaxConversionPerMonth { get; set; }

        [Required(ErrorMessage = "Dimensiunea maximă per fișier este obligatorie.")]
        [Range(1, long.MaxValue, ErrorMessage = "Dimensiunea trebuie să fie mai mare ca 0.")]
        [Display(Name = "Max upload (bytes)")]
        public long MaxUploadBytes { get; set; }

        [StringLength(255, ErrorMessage = "Descrierea nu poate depăși 255 caractere.")]
        [Display(Name = "Descriere")]
        public string? Description { get; set; }

        [Display(Name = "Activ")]
        public bool IsActive { get; set; } = true;
    }
}
