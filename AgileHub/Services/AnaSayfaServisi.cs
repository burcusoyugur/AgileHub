using AgileHub.Data;
using AgileHub.Models;
using Microsoft.EntityFrameworkCore;

namespace AgileHub.Services
{
    // Ana sayfa panosunun verilerini toplar.
    // HttpContext kullanmaz; kullanıcıyı parametre olarak alır. Böylece ileride
    // mobil API de (ör. "Görevlerim") aynı sorguları kullanabilir.
    public class AnaSayfaServisi
    {
        private readonly ApplicationDbContext _db;
        private readonly ProjeYetkiServisi _yetki;

        public AnaSayfaServisi(ApplicationDbContext db, ProjeYetkiServisi yetki)
        {
            _db = db;
            _yetki = yetki;
        }

        // Kullanıcıya atanmış, bitmemiş görevler (sadece üyesi olduğu projelerden).
        public IQueryable<Gorev> BanaAtananAcikGorevler(string kullaniciId)
        {
            return _db.Gorevler.Where(g =>
                g.AtananKullaniciId == kullaniciId &&
                g.Durum != GorevDurumu.Bitti &&
                g.Proje!.ProjeUyeleri.Any(u => u.KullaniciId == kullaniciId));
        }

        public async Task<HomeIndexViewModel> PanoyuGetirAsync(
            string kullaniciId, int gorevSayisi = 8, int sonProjeSayisi = 4)
        {
            var projelerim = _yetki.UyesiOldugumProjeler(kullaniciId);

            var adSoyad = await _db.Users
                .Where(u => u.Id == kullaniciId)
                .Select(u => u.Ad + " " + u.Soyad)
                .FirstOrDefaultAsync();

            var projeSayisi = await projelerim.CountAsync();

            var acikGorevler = BanaAtananAcikGorevler(kullaniciId);
            var acikGorevSayisi = await acikGorevler.CountAsync();

            // En yüksek öncelik önce (Kritik > Yuksek > Orta > Dusuk), eşitse en eski önce.
            var gorevler = await acikGorevler
                .Include(g => g.Proje)
                .OrderByDescending(g => g.Oncelik)
                .ThenBy(g => g.OlusturmaTarihi)
                .Take(gorevSayisi)
                .AsNoTracking()
                .ToListAsync();

            // Üyesi olduğum projelerdeki aktif sprintler, bitişi en yakın olan önce.
            var aktifSprintler = await _db.Sprintler
                .Where(s => s.Durum == SprintDurumu.Aktif &&
                            projelerim.Any(p => p.Id == s.ProjeId))
                .OrderBy(s => s.BitisTarihi)
                .Select(s => new AktifSprintOzeti
                {
                    Id = s.Id,
                    ProjeId = s.ProjeId,
                    ProjeAdi = s.Proje!.Ad,
                    Ad = s.Ad,
                    BitisTarihi = s.BitisTarihi,
                    ToplamStoryPoint = s.Gorevler.Sum(g => g.StoryPoint ?? 0),
                    TamamlananStoryPoint = s.Gorevler
                        .Where(g => g.Durum == GorevDurumu.Bitti)
                        .Sum(g => g.StoryPoint ?? 0)
                })
                .ToListAsync();

            var sonProjeler = await projelerim
                .OrderByDescending(p => p.OlusturmaTarihi)
                .Take(sonProjeSayisi)
                .AsNoTracking()
                .ToListAsync();

            return new HomeIndexViewModel
            {
                AdSoyad = adSoyad?.Trim() ?? string.Empty,
                ProjeSayisi = projeSayisi,
                AcikGorevSayisi = acikGorevSayisi,
                AktifSprintler = aktifSprintler,
                BanaAtananGorevler = gorevler,
                SonProjeler = sonProjeler
            };
        }
    }
}
