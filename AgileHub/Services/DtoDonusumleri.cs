using AgileHub.Models;

namespace AgileHub.Services
{
    // Entity → API DTO dönüşümleri tek yerde. Mobil uygulama DTO alan adlarını birebir okur.
    // Kişi adları ve story point toplamları gezinme özelliklerinden hesaplanır;
    // sorguda ilgili Include yapılmazsa ad null, toplamlar 0 gelir.
    public static class DtoDonusumleri
    {
        private static string? AdSoyad(ApplicationUser? kullanici)
        {
            return kullanici is null ? null : $"{kullanici.Ad} {kullanici.Soyad}".Trim();
        }

        // Include: OlusturanKullanici
        public static ProjeDto ToDto(this Proje proje)
        {
            return new ProjeDto(
                proje.Id,
                proje.Ad,
                proje.Aciklama,
                proje.OlusturmaTarihi,
                proje.OlusturanKullaniciId,
                AdSoyad(proje.OlusturanKullanici));
        }

        // Include: Gorevler
        public static SprintDto ToDto(this Sprint sprint)
        {
            return new SprintDto(
                sprint.Id,
                sprint.ProjeId,
                sprint.Ad,
                sprint.Hedef,
                sprint.BaslangicTarihi,
                sprint.BitisTarihi,
                sprint.Durum,
                sprint.Gorevler.Sum(g => g.StoryPoint ?? 0),
                sprint.Gorevler.Where(g => g.Durum == GorevDurumu.Bitti).Sum(g => g.StoryPoint ?? 0));
        }

        // Include: AtananKullanici
        public static GorevDto ToDto(this Gorev gorev)
        {
            return new GorevDto(
                gorev.Id,
                gorev.ProjeId,
                gorev.SprintId,
                gorev.Baslik,
                gorev.Aciklama,
                gorev.Durum,
                gorev.Oncelik,
                gorev.StoryPoint,
                gorev.AtananKullaniciId,
                AdSoyad(gorev.AtananKullanici),
                gorev.OlusturmaTarihi);
        }

        // Include: YazanKullanici
        public static YorumDto ToDto(this Yorum yorum)
        {
            return new YorumDto(
                yorum.Id,
                yorum.GorevId,
                yorum.YazanKullaniciId,
                AdSoyad(yorum.YazanKullanici) ?? string.Empty,
                yorum.Metin,
                yorum.OlusturmaTarihi);
        }
    }
}
