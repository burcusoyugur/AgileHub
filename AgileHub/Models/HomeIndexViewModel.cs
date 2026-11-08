namespace AgileHub.Models
{
    // Ana sayfadaki panonun (dashboard) ihtiyaç duyduğu bütün veriler tek pakette.
    public class HomeIndexViewModel
    {
        public string AdSoyad { get; set; } = string.Empty;

        // Üstteki özet kartları
        public int ProjeSayisi { get; set; }
        public int AcikGorevSayisi { get; set; }
        public int AktifSprintSayisi => AktifSprintler.Count;

        public List<AktifSprintOzeti> AktifSprintler { get; set; } = new();
        public List<Gorev> BanaAtananGorevler { get; set; } = new();
        public List<Proje> SonProjeler { get; set; } = new();
    }

    // Bir aktif sprintin panoda gösterilecek özeti.
    public class AktifSprintOzeti
    {
        public int Id { get; set; }
        public int ProjeId { get; set; }
        public string ProjeAdi { get; set; } = string.Empty;
        public string Ad { get; set; } = string.Empty;
        public DateTime BitisTarihi { get; set; }
        public int ToplamStoryPoint { get; set; }
        public int TamamlananStoryPoint { get; set; }

        // Bunlar veritabanından gelmez, yukarıdaki değerlerden hesaplanır.
        public int KalanGun => Math.Max(0, (BitisTarihi.Date - DateTime.Today).Days);

        public int IlerlemeYuzdesi => ToplamStoryPoint == 0
            ? 0
            : TamamlananStoryPoint * 100 / ToplamStoryPoint;
    }
}
