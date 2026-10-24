using System.ComponentModel.DataAnnotations;

namespace AgileHub.Models
{
    public enum SprintDurumu
    {
        Planlanıyor,
        Aktif,
        Tamamlandı,
        İptalEdildi
    }

    public class Sprint
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Ad { get; set; } = string.Empty;

        public string? Hedef { get; set; }

        public DateTime BaslangicTarihi { get; set; }
        public DateTime BitisTarihi { get; set; }

        public SprintDurumu Durum { get; set; } = SprintDurumu.Planlanıyor;

        public int ProjeId { get; set; }
        public Proje? Proje { get; set; }

        public ICollection<Gorev> Gorevler { get; set; } = new List<Gorev>();
    }
}
