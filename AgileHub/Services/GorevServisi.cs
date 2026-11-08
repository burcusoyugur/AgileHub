using AgileHub.Data;
using AgileHub.Models;
using Microsoft.EntityFrameworkCore;

namespace AgileHub.Services
{
    // Görev (backlog) kuralları burada; web GorevController'ı ve ileride
    // /api/gorevler uçları bu servisi kullanır.
    public class GorevServisi
    {
        private readonly ApplicationDbContext _db;
        private readonly ProjeYetkiServisi _yetki;

        public GorevServisi(ApplicationDbContext db, ProjeYetkiServisi yetki)
        {
            _db = db;
            _yetki = yetki;
        }

        // Projenin görevleri: önce en yüksek öncelik, aynı öncelikte en yeni görev önce
        // (aynı anda eklenenlerde sıra sabit kalsın diye son olarak Id'ye bakılır).
        // sadeceBacklog true ise yalnızca sprinte atanmamış görevler gelir.
        // Durum ve atanan kişi filtreleri isteğe bağlıdır.
        // AtananKullanici (DTO'daki ad için) ve Sprint (listede sprint adı için) yüklenir.
        public async Task<IslemSonucu<List<Gorev>>> ProjeGorevleriniGetirAsync(
            int projeId,
            string kullaniciId,
            bool sadeceBacklog = false,
            GorevDurumu? durum = null,
            string? atananKullaniciId = null)
        {
            if (!await _db.Projeler.AnyAsync(p => p.Id == projeId))
            {
                return IslemSonucu.Bulunamadi();
            }

            if (!await _yetki.UyeMiAsync(projeId, kullaniciId))
            {
                return IslemSonucu.Yetkisiz();
            }

            var sorgu = _db.Gorevler.Where(g => g.ProjeId == projeId);

            if (sadeceBacklog)
            {
                sorgu = sorgu.Where(g => g.SprintId == null);
            }

            if (durum is GorevDurumu d)
            {
                sorgu = sorgu.Where(g => g.Durum == d);
            }

            if (!string.IsNullOrEmpty(atananKullaniciId))
            {
                sorgu = sorgu.Where(g => g.AtananKullaniciId == atananKullaniciId);
            }

            var gorevler = await sorgu
                .Include(g => g.AtananKullanici)
                .Include(g => g.Sprint)
                .OrderByDescending(g => g.Oncelik)
                .ThenByDescending(g => g.OlusturmaTarihi)
                .ThenByDescending(g => g.Id)
                .AsNoTracking()
                .ToListAsync();

            return IslemSonucu<List<Gorev>>.Basarili(gorevler);
        }

        // Detay, düzenleme ve silme sayfaları için. Yorumlar silme onayında sayı göstermek için yüklenir.
        public async Task<IslemSonucu<Gorev>> GetirAsync(int gorevId, string kullaniciId)
        {
            var gorev = await _db.Gorevler
                .Include(g => g.Proje)
                .Include(g => g.Sprint)
                .Include(g => g.AtananKullanici)
                .Include(g => g.Yorumlar)
                .AsSplitQuery()
                .AsNoTracking()
                .FirstOrDefaultAsync(g => g.Id == gorevId);

            if (gorev is null)
            {
                return IslemSonucu.Bulunamadi();
            }

            if (!await _yetki.UyeMiAsync(gorev.ProjeId, kullaniciId))
            {
                return IslemSonucu.Yetkisiz();
            }

            return IslemSonucu<Gorev>.Basarili(gorev);
        }

        // Görev atanabilecek kişiler (projenin üyeleri), ada göre sıralı.
        // Formdaki "Atanan" listesi ve ileride mobil görev formu için.
        public async Task<IslemSonucu<List<ApplicationUser>>> AtanabilecekKisileriGetirAsync(int projeId, string kullaniciId)
        {
            if (!await _db.Projeler.AnyAsync(p => p.Id == projeId))
            {
                return IslemSonucu.Bulunamadi();
            }

            if (!await _yetki.UyeMiAsync(projeId, kullaniciId))
            {
                return IslemSonucu.Yetkisiz();
            }

            var kisiler = await _db.ProjeUyeleri
                .Where(u => u.ProjeId == projeId)
                .Select(u => u.Kullanici!)
                .OrderBy(k => k.Ad)
                .ThenBy(k => k.Soyad)
                .AsNoTracking()
                .ToListAsync();

            return IslemSonucu<List<ApplicationUser>>.Basarili(kisiler);
        }
    }
}
