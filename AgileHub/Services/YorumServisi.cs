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
