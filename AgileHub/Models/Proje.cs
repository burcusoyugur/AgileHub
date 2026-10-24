using System.ComponentModel.DataAnnotations;

namespace AgileHub.Models
{
    public class Proje
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Ad { get; set; } = string.Empty;

        public string? Aciklama { get; set; }

        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;

        public string OlusturanKullaniciId { get; set; } = string.Empty;
        public ApplicationUser? OlusturanKullanici { get; set; }

        public ICollection<ProjeUyesi> ProjeUyeleri { get; set; } = new List<ProjeUyesi>();
        public ICollection<Sprint> Sprintler { get; set; } = new List<Sprint>();
        public ICollection<Gorev> Gorevler { get; set; } = new List<Gorev>();
    }
}
