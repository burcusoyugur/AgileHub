using System.ComponentModel.DataAnnotations;

namespace AgileHub.Models
{
    public enum GorevDurumu
    {
        Yapilacak,
        Yapiliyor,
        Test,
        Bitti
    }

    public enum GorevOnceligi
    {
        Dusuk,
        Orta,
        Yuksek,
        Kritik
    }

    public class Gorev
    {
        public int Id { get; set; }

        [Required, MaxLength(300)]
        public string Baslik { get; set; } = string.Empty;

        public string? Aciklama { get; set; }

        public GorevDurumu Durum { get; set; } = GorevDurumu.Yapilacak;
        public GorevOnceligi Oncelik { get; set; } = GorevOnceligi.Orta;

        public int? StoryPoint { get; set; }

        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;

        public int ProjeId { get; set; }
        public Proje? Proje { get; set; }

        public int? SprintId { get; set; }
        public Sprint? Sprint { get; set; }

        public string? AtananKullaniciId { get; set; }
        public ApplicationUser? AtananKullanici { get; set; }

        public ICollection<Yorum> Yorumlar { get; set; } = new List<Yorum>();
        public ICollection<GorevEtiket> GorevEtiketler { get; set; } = new List<GorevEtiket>();
    }
}
