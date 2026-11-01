using System.ComponentModel.DataAnnotations;

namespace AgileHub.Models
{
    // Oluşturma ve düzenleme formunda yalnızca bu alanlar alınır.
    // OlusturanKullaniciId gibi alanlar formdan gelmez, controller'da atanır.
    public class ProjeFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Proje adı zorunludur.")]
        [MaxLength(200, ErrorMessage = "Proje adı en fazla 200 karakter olabilir.")]
        [Display(Name = "Proje adı")]
        public string Ad { get; set; } = string.Empty;

        [Display(Name = "Açıklama")]
        public string? Aciklama { get; set; }
    }
}
