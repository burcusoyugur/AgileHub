namespace AgileHub.Models
{
    // Kanban panosu sayfasının verisi: seçili sprintin görevleri durumlarına göre sütunlarda.
    public class PanoViewModel
    {
        public int ProjeId { get; set; }

        public string ProjeAdi { get; set; } = string.Empty;

        // Sprint seçimi için projenin bütün sprintleri.
        public List<Sprint> Sprintler { get; set; } = new();

        // sprintId verilmemiş ve projede aktif sprint yoksa null olur.
        public Sprint? SeciliSprint { get; set; }

        // Her durum için bir sütun, GorevDurumu sırasıyla. Seçili sprint yoksa boştur.
        public List<PanoSutunu> Sutunlar { get; set; } = new();

        public int KalanGun => SeciliSprint is null
            ? 0
            : Math.Max(0, (SeciliSprint.BitisTarihi.Date - DateTime.Today).Days);

        public int ToplamStoryPoint => Sutunlar.Sum(s => s.ToplamStoryPoint);

        public int TamamlananStoryPoint => Sutunlar
            .Where(s => s.Durum == GorevDurumu.Bitti)
            .Sum(s => s.ToplamStoryPoint);

        public int IlerlemeYuzdesi => ToplamStoryPoint == 0
            ? 0
            : TamamlananStoryPoint * 100 / ToplamStoryPoint;

        // Sütun içinde en yüksek öncelik önce, aynı öncelikte en eski görev önce.
        public static List<PanoSutunu> SutunlaraAyir(IEnumerable<Gorev> gorevler)
        {
            return Enum.GetValues<GorevDurumu>()
                .Select(durum => new PanoSutunu(
                    durum,
                    gorevler
                        .Where(g => g.Durum == durum)
                        .OrderByDescending(g => g.Oncelik)
                        .ThenBy(g => g.OlusturmaTarihi)
                        .ThenBy(g => g.Id)
                        .ToList()))
                .ToList();
        }
    }

    // Panodaki tek bir sütun. Boş story point 0 sayılır.
    public record PanoSutunu(GorevDurumu Durum, List<Gorev> Gorevler)
    {
        public int ToplamStoryPoint => Gorevler.Sum(g => g.StoryPoint ?? 0);
    }
}
