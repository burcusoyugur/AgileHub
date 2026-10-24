using System.ComponentModel.DataAnnotations;

namespace AgileHub.Models
{
    public class Yorum
    {
        public int Id { get; set; }

        [Required]
        public string Metin { get; set; } = string.Empty;

        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;

        public int GorevId { get; set; }
        public Gorev? Gorev { get; set; }

        public string YazanKullaniciId { get; set; } = string.Empty;
        public ApplicationUser? YazanKullanici { get; set; }
    }
}
