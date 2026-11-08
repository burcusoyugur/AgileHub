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

        // Form ve API aynı kuralı kullansın diye alan doğrulaması da burada yapılır.
        private static string? AlanHatasi(string baslik, GorevOnceligi oncelik, GorevDurumu durum, int? storyPoint)
        {
            if (string.IsNullOrWhiteSpace(baslik))
            {
                return "Görev başlığı zorunludur.";
            }

            if (baslik.Trim().Length > 300)
            {
                return "Görev başlığı en fazla 300 karakter olabilir.";
            }

            // API'den sayı olarak tanımsız bir değer gelebilir.
            if (!Enum.IsDefined(oncelik))
            {
                return "Geçersiz öncelik.";
            }

            if (!Enum.IsDefined(durum))
            {
                return "Geçersiz durum.";
            }

            if (storyPoint is < 0 or > 100)
            {
                return "Story point 0 ile 100 arasında olmalıdır.";
            }

            return null;
        }

        // Atanan kişi boş bırakılabilir; seçildiyse projenin üyesi olmalıdır.
        private async Task<string?> AtananHatasiAsync(int projeId, string? atananKullaniciId)
        {
            if (atananKullaniciId is null || await _yetki.UyeMiAsync(projeId, atananKullaniciId))
            {
                return null;
            }

            return "Görev yalnızca projenin üyelerinden birine atanabilir.";
        }

        private static string? BosIseNull(string? metin)
        {
            return string.IsNullOrWhiteSpace(metin) ? null : metin.Trim();
        }

        // Yeni görev her zaman Yapılacak durumunda ve backlog'da (sprintsiz) başlar.
        public async Task<IslemSonucu<Gorev>> OlusturAsync(
            int projeId,
            string baslik,
            string? aciklama,
            GorevOnceligi oncelik,
            int? storyPoint,
            string? atananKullaniciId,
            string kullaniciId)
        {
            if (!await _db.Projeler.AnyAsync(p => p.Id == projeId))
            {
                return IslemSonucu.Bulunamadi();
            }

            if (!await _yetki.UyeMiAsync(projeId, kullaniciId))
            {
                return IslemSonucu.Yetkisiz();
            }

            atananKullaniciId = BosIseNull(atananKullaniciId);

            if (AlanHatasi(baslik, oncelik, GorevDurumu.Yapilacak, storyPoint) is string hata)
            {
                return IslemSonucu.Gecersiz(hata);
            }

            if (await AtananHatasiAsync(projeId, atananKullaniciId) is string atananHatasi)
            {
                return IslemSonucu.Gecersiz(atananHatasi);
            }

            var gorev = new Gorev
            {
                ProjeId = projeId,
                Baslik = baslik.Trim(),
                Aciklama = BosIseNull(aciklama),
                Oncelik = oncelik,
                Durum = GorevDurumu.Yapilacak,
                StoryPoint = storyPoint,
                AtananKullaniciId = atananKullaniciId,
                SprintId = null,
                OlusturmaTarihi = DateTime.Now
            };

            _db.Gorevler.Add(gorev);
            await _db.SaveChangesAsync();

            return IslemSonucu<Gorev>.Basarili(gorev);
        }

        // Görevin sprinti burada değişmez; sprinte atama SprintAtamaServisi'nin işidir.
        public async Task<IslemSonucu> GuncelleAsync(
            int gorevId,
            string baslik,
            string? aciklama,
            GorevOnceligi oncelik,
            GorevDurumu durum,
            int? storyPoint,
            string? atananKullaniciId,
            string kullaniciId)
        {
            var gorev = await _db.Gorevler.FirstOrDefaultAsync(g => g.Id == gorevId);

            if (gorev is null)
            {
                return IslemSonucu.Bulunamadi();
            }

            if (!await _yetki.UyeMiAsync(gorev.ProjeId, kullaniciId))
            {
                return IslemSonucu.Yetkisiz();
            }

            atananKullaniciId = BosIseNull(atananKullaniciId);

            if (AlanHatasi(baslik, oncelik, durum, storyPoint) is string hata)
            {
                return IslemSonucu.Gecersiz(hata);
            }

            if (await AtananHatasiAsync(gorev.ProjeId, atananKullaniciId) is string atananHatasi)
            {
                return IslemSonucu.Gecersiz(atananHatasi);
            }

            gorev.Baslik = baslik.Trim();
            gorev.Aciklama = BosIseNull(aciklama);
            gorev.Oncelik = oncelik;
            gorev.Durum = durum;
            gorev.StoryPoint = storyPoint;
            gorev.AtananKullaniciId = atananKullaniciId;
            await _db.SaveChangesAsync();

            return IslemSonucu.Basarili();
        }

        // Görevin yorumları ve etiket bağlantıları veritabanında cascade ile silinir.
        public async Task<IslemSonucu> SilAsync(int gorevId, string kullaniciId)
        {
            var gorev = await _db.Gorevler.FirstOrDefaultAsync(g => g.Id == gorevId);

            if (gorev is null)
            {
                return IslemSonucu.Bulunamadi();
            }

            if (!await _yetki.UyeMiAsync(gorev.ProjeId, kullaniciId))
            {
                return IslemSonucu.Yetkisiz();
            }

            _db.Gorevler.Remove(gorev);
            await _db.SaveChangesAsync();

            return IslemSonucu.Basarili();
        }
    }
}
