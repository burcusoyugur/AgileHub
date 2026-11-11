using AgileHub.Data;
using AgileHub.Models;
using Microsoft.EntityFrameworkCore;

namespace AgileHub.Services
{
    // Projeleri okuma işlemleri (API için). Oluşturma, düzenleme ve üye yönetimi
    // şimdilik web'deki ProjeController ve ProjeUyeleriController'da.
    public class ProjeServisi
    {
        private readonly ApplicationDbContext _db;
        private readonly ProjeYetkiServisi _yetki;

        public ProjeServisi(ApplicationDbContext db, ProjeYetkiServisi yetki)
        {
            _db = db;
            _yetki = yetki;
        }

        // Kullanıcının üyesi olduğu projeler, en yeni oluşturulan önce.
        // OlusturanKullanici, DTO'daki ad için yüklenir.
        public Task<List<Proje>> UyesiOldugumProjeleriGetirAsync(string kullaniciId)
        {
            return _yetki.UyesiOldugumProjeler(kullaniciId)
                .Include(p => p.OlusturanKullanici)
                .OrderByDescending(p => p.OlusturmaTarihi)
                .ThenByDescending(p => p.Id)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IslemSonucu<Proje>> GetirAsync(int projeId, string kullaniciId)
        {
            var proje = await _db.Projeler
                .Include(p => p.OlusturanKullanici)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == projeId);

            if (proje is null)
            {
                return IslemSonucu.Bulunamadi();
            }

            if (!await _yetki.UyeMiAsync(projeId, kullaniciId))
            {
                return IslemSonucu.Yetkisiz();
            }

            return IslemSonucu<Proje>.Basarili(proje);
        }
    }
}
