using System.ComponentModel.DataAnnotations;

namespace AgileHub.Models
{
    // Sprint oluşturma ve düzenleme formu. Durum formdan gelmez;
    // Başlat/Tamamla işlemleriyle değişir (bkz. SprintServisi).
    public class SprintFormViewModel : IValidatableObject
    {
        public int Id { get; set; }

        public int ProjeId { get; set; }

        [Required(ErrorMessage = "Sprint adı zorunludur.")]
        [MaxLength(200, ErrorMessage = "Sprint adı en fazla 200 karakter olabilir.")]
        [Display(Name = "Sprint adı")]
        public string Ad { get; set; } = string.Empty;

        [Display(Name = "Sprint hedefi")]
        public string? Hedef { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Başlangıç tarihi")]
        public DateTime BaslangicTarihi { get; set; } = DateTime.Today;

        // Varsayılan sprint süresi iki hafta.
        [DataType(DataType.Date)]
        [Display(Name = "Bitiş tarihi")]
        public DateTime BitisTarihi { get; set; } = DateTime.Today.AddDays(14);

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (BitisTarihi.Date <= BaslangicTarihi.Date)
            {
                yield return new ValidationResult(
                    "Bitiş tarihi başlangıç tarihinden sonra olmalıdır.",
                    [nameof(BitisTarihi)]);
            }
        }
    }
}
