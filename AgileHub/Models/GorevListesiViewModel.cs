using Microsoft.AspNetCore.Mvc.Rendering;

namespace AgileHub.Models
{
    // Backlog sayfası: projenin görevleri ve seçili filtreler.
    public class GorevListesiViewModel
    {
        public int ProjeId { get; set; }

        public string ProjeAdi { get; set; } = string.Empty;

        // true: yalnızca sprinte atanmamış görevler (backlog), false: projenin tüm görevleri.
        public bool SadeceBacklog { get; set; } = true;

        public GorevDurumu? Durum { get; set; }

        public string? AtananKullaniciId { get; set; }

        public List<Gorev> Gorevler { get; set; } = [];

        // Atanan kişi filtresi için projenin üyeleri.
        public IEnumerable<SelectListItem> AtanabilecekKisiler { get; set; } = [];

        public bool FiltreVarMi => Durum is not null || !string.IsNullOrEmpty(AtananKullaniciId);

        public int ToplamStoryPoint => Gorevler.Sum(g => g.StoryPoint ?? 0);
    }
}
