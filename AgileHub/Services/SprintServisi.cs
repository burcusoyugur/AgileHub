using AgileHub.Data;
using AgileHub.Models;
using Microsoft.EntityFrameworkCore;

namespace AgileHub.Services
{
    // Sprint kuralları burada; web SprintController'ı ve ileride API uçları bu servisi kullanır.
    public class SprintServisi
    {
        private readonly ApplicationDbContext _db;
        private readonly ProjeYetkiServisi _yetki;

        public SprintServisi(ApplicationDbContext db, ProjeYetkiServisi yetki)
        {
            _db = db;
            _yetki = yetki;
        }

        // Projenin sprintleri, en yeni başlangıç tarihi önce.
        // Görevler story point toplamları için yüklenir (bkz. DtoDonusumleri).
        public async Task<IslemSonucu<List<Sprint>>> ProjeSprintleriniGetirAsync(int projeId, string kullaniciId)
        {
            if (!await _db.Projeler.AnyAsync(p => p.Id == projeId))
            {
                return IslemSonucu.Bulunamadi();
            }

            if (!await _yetki.UyeMiAsync(projeId, kullaniciId))
            {
                return IslemSonucu.Yetkisiz();
            }

            var sprintler = await _db.Sprintler
                .Where(s => s.ProjeId == projeId)
                .Include(s => s.Gorevler)
                .OrderByDescending(s => s.BaslangicTarihi)
                .AsNoTracking()
                .ToListAsync();

            return IslemSonucu<List<Sprint>>.Basarili(sprintler);
        }

        public async Task<IslemSonucu<Sprint>> GetirAsync(int sprintId, string kullaniciId)
        {
            var sprint = await _db.Sprintler
                .Include(s => s.Proje)
                .Include(s => s.Gorevler).ThenInclude(g => g.AtananKullanici)
                .AsSplitQuery()
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == sprintId);

            if (sprint is null)
            {
                return IslemSonucu.Bulunamadi();
            }

            if (!await _yetki.UyeMiAsync(sprint.ProjeId, kullaniciId))
            {
                return IslemSonucu.Yetkisiz();
            }

            return IslemSonucu<Sprint>.Basarili(sprint);
        }

        // Form ve API aynı kuralı kullansın diye alan doğrulaması da burada yapılır.
        private static string? AlanHatasi(string ad, DateTime baslangic, DateTime bitis)
        {
            if (string.IsNullOrWhiteSpace(ad))
            {
                return "Sprint adı zorunludur.";
            }

            if (ad.Trim().Length > 200)
            {
                return "Sprint adı en fazla 200 karakter olabilir.";
            }

            if (bitis.Date <= baslangic.Date)
            {
                return "Bitiş tarihi başlangıç tarihinden sonra olmalıdır.";
            }

            return null;
        }

        // Yeni sprint her zaman Planlanıyor durumunda başlar.
        public async Task<IslemSonucu<Sprint>> OlusturAsync(
            int projeId, string ad, string? hedef, DateTime baslangic, DateTime bitis, string kullaniciId)
        {
            if (!await _db.Projeler.AnyAsync(p => p.Id == projeId))
            {
                return IslemSonucu.Bulunamadi();
            }

            if (!await _yetki.UyeMiAsync(projeId, kullaniciId))
            {
                return IslemSonucu.Yetkisiz();
            }

            if (AlanHatasi(ad, baslangic, bitis) is string hata)
            {
                return IslemSonucu.Gecersiz(hata);
            }

            var sprint = new Sprint
            {
                ProjeId = projeId,
                Ad = ad.Trim(),
                Hedef = hedef,
                BaslangicTarihi = baslangic.Date,
                BitisTarihi = bitis.Date,
                Durum = SprintDurumu.Planlanıyor
            };

            _db.Sprintler.Add(sprint);
            await _db.SaveChangesAsync();

            return IslemSonucu<Sprint>.Basarili(sprint);
        }

        public async Task<IslemSonucu> GuncelleAsync(
            int sprintId, string ad, string? hedef, DateTime baslangic, DateTime bitis, string kullaniciId)
        {
            var sprint = await _db.Sprintler.FirstOrDefaultAsync(s => s.Id == sprintId);

            if (sprint is null)
            {
                return IslemSonucu.Bulunamadi();
            }

            if (!await _yetki.UyeMiAsync(sprint.ProjeId, kullaniciId))
            {
                return IslemSonucu.Yetkisiz();
            }

            if (sprint.Durum is SprintDurumu.Tamamlandı or SprintDurumu.İptalEdildi)
            {
                return IslemSonucu.Gecersiz("Tamamlanmış veya iptal edilmiş sprint düzenlenemez.");
            }

            if (AlanHatasi(ad, baslangic, bitis) is string hata)
            {
                return IslemSonucu.Gecersiz(hata);
            }

            sprint.Ad = ad.Trim();
            sprint.Hedef = hedef;
            sprint.BaslangicTarihi = baslangic.Date;
            sprint.BitisTarihi = bitis.Date;
            await _db.SaveChangesAsync();

            return IslemSonucu.Basarili();
        }

        // Sprintteki görevler silinmez, backlog'a döner.
        public async Task<IslemSonucu> SilAsync(int sprintId, string kullaniciId)
        {
            var sprint = await _db.Sprintler
                .Include(s => s.Gorevler)
                .FirstOrDefaultAsync(s => s.Id == sprintId);

            if (sprint is null)
            {
                return IslemSonucu.Bulunamadi();
            }

            if (!await _yetki.UyeMiAsync(sprint.ProjeId, kullaniciId))
            {
                return IslemSonucu.Yetkisiz();
            }

            // Gorev -> Sprint ilişkisi NoAction olduğu için bağlantı önce elle kaldırılır.
            foreach (var gorev in sprint.Gorevler)
            {
                gorev.SprintId = null;
            }

            _db.Sprintler.Remove(sprint);
            await _db.SaveChangesAsync();

            return IslemSonucu.Basarili();
        }

        // Bir projede aynı anda yalnızca bir aktif sprint olabilir.
        public async Task<IslemSonucu> BaslatAsync(int sprintId, string kullaniciId)
        {
            var sprint = await _db.Sprintler.FirstOrDefaultAsync(s => s.Id == sprintId);

            if (sprint is null)
            {
                return IslemSonucu.Bulunamadi();
            }

            if (!await _yetki.UyeMiAsync(sprint.ProjeId, kullaniciId))
            {
                return IslemSonucu.Yetkisiz();
            }

            if (sprint.Durum != SprintDurumu.Planlanıyor)
            {
                return IslemSonucu.Gecersiz("Yalnızca planlanan bir sprint başlatılabilir.");
            }

            var aktifVarMi = await _db.Sprintler
                .AnyAsync(s => s.ProjeId == sprint.ProjeId && s.Durum == SprintDurumu.Aktif);

            if (aktifVarMi)
            {
                return IslemSonucu.Gecersiz("Projede zaten aktif bir sprint var. Önce onu tamamlayın.");
            }

            sprint.Durum = SprintDurumu.Aktif;
            await _db.SaveChangesAsync();

            return IslemSonucu.Basarili();
        }

        // Bitmemiş görevler backlog'a döner; değer olarak geri dönen görev sayısı verilir.
        public async Task<IslemSonucu<int>> TamamlaAsync(int sprintId, string kullaniciId)
        {
            var sprint = await _db.Sprintler
                .Include(s => s.Gorevler)
                .FirstOrDefaultAsync(s => s.Id == sprintId);

            if (sprint is null)
            {
                return IslemSonucu.Bulunamadi();
            }

            if (!await _yetki.UyeMiAsync(sprint.ProjeId, kullaniciId))
            {
                return IslemSonucu.Yetkisiz();
            }

            if (sprint.Durum != SprintDurumu.Aktif)
            {
                return IslemSonucu.Gecersiz("Yalnızca aktif bir sprint tamamlanabilir.");
            }

            var bitmeyenler = sprint.Gorevler.Where(g => g.Durum != GorevDurumu.Bitti).ToList();

            foreach (var gorev in bitmeyenler)
            {
                gorev.SprintId = null;
            }

            sprint.Durum = SprintDurumu.Tamamlandı;
            await _db.SaveChangesAsync();

            return IslemSonucu<int>.Basarili(bitmeyenler.Count);
        }
    }
}
