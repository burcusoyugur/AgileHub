using AgileHub.Data;
using AgileHub.Models;
using Microsoft.EntityFrameworkCore;

namespace AgileHub.Services
{
    // Görev yorumlarının kuralları burada; web (YorumController, Yorumlar bileşeni)
    // ve ileride mobilin yorum API'si aynı servisi kullanır.
    // Okumak ve yazmak için projenin üyesi olmak, silmek için yorumu yazmış olmak gerekir.
    public class YorumServisi
    {
        public const int MetinUzunlukSiniri = 2000;

        private readonly ApplicationDbContext _db;
        private readonly ProjeYetkiServisi _yetki;

        public YorumServisi(ApplicationDbContext db, ProjeYetkiServisi yetki)
        {
            _db = db;
            _yetki = yetki;
        }

        // Görevin yorumları eskiden yeniye; yazan kişinin adı için YazanKullanici yüklenir.
        public async Task<IslemSonucu<List<Yorum>>> GorevYorumlariniGetirAsync(int gorevId, string kullaniciId)
        {
            var yetkiSonucu = await GoreveErisimAsync(gorevId, kullaniciId);

            if (!yetkiSonucu.BasariliMi)
            {
                return yetkiSonucu;
            }

            var yorumlar = await _db.Yorumlar
                .Where(y => y.GorevId == gorevId)
                .Include(y => y.YazanKullanici)
                .OrderBy(y => y.OlusturmaTarihi)
                .ThenBy(y => y.Id)
                .AsNoTracking()
                .ToListAsync();

            return IslemSonucu<List<Yorum>>.Basarili(yorumlar);
        }

        // Uzunluk sınırı veritabanında değil, burada uygulanır (şema değişmiyor).
        public async Task<IslemSonucu<Yorum>> EkleAsync(int gorevId, string? metin, string kullaniciId)
        {
            var yetkiSonucu = await GoreveErisimAsync(gorevId, kullaniciId);

            if (!yetkiSonucu.BasariliMi)
            {
                return yetkiSonucu;
            }

            var temizMetin = metin?.Trim() ?? string.Empty;

            if (temizMetin.Length == 0)
            {
                return IslemSonucu.Gecersiz("Yorum boş olamaz.");
            }

            if (temizMetin.Length > MetinUzunlukSiniri)
            {
                return IslemSonucu.Gecersiz($"Yorum en fazla {MetinUzunlukSiniri} karakter olabilir.");
            }

            var yorum = new Yorum
            {
                GorevId = gorevId,
                Metin = temizMetin,
                YazanKullaniciId = kullaniciId,
                OlusturmaTarihi = DateTime.Now
            };

            _db.Yorumlar.Add(yorum);
            await _db.SaveChangesAsync();

            // API'de yazan kişinin adı dönebilsin diye.
            await _db.Entry(yorum).Reference(y => y.YazanKullanici).LoadAsync();

            return IslemSonucu<Yorum>.Basarili(yorum);
        }

        // Başarılıysa yorumun bağlı olduğu görevin id'si döner (silindikten sonra detaya dönmek için).
        public async Task<IslemSonucu<int>> SilAsync(int yorumId, string kullaniciId)
        {
            var yorum = await _db.Yorumlar.FirstOrDefaultAsync(y => y.Id == yorumId);

            if (yorum is null)
            {
                return IslemSonucu.Bulunamadi();
            }

            if (yorum.YazanKullaniciId != kullaniciId)
            {
                return IslemSonucu.Yetkisiz();
            }

            var gorevId = yorum.GorevId;

            _db.Yorumlar.Remove(yorum);
            await _db.SaveChangesAsync();

            return IslemSonucu<int>.Basarili(gorevId);
        }

        // Görev yoksa 404, kullanıcı görevin projesine üye değilse 403.
        private async Task<IslemSonucu> GoreveErisimAsync(int gorevId, string kullaniciId)
        {
            var projeId = await _db.Gorevler
                .Where(g => g.Id == gorevId)
                .Select(g => (int?)g.ProjeId)
                .FirstOrDefaultAsync();

            if (projeId is null)
            {
                return IslemSonucu.Bulunamadi();
            }

            if (!await _yetki.UyeMiAsync(projeId.Value, kullaniciId))
            {
                return IslemSonucu.Yetkisiz();
            }

            return IslemSonucu.Basarili();
        }
    }
}
