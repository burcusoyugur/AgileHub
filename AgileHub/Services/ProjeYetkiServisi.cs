using AgileHub.Data;
using AgileHub.Models;
using Microsoft.EntityFrameworkCore;

namespace AgileHub.Services
{
    // Proje erişim kuralları tek yerde: web controller'ları ve API uçları aynı kontrolü kullanır.
    // Proje yoksa da false döner; 404 ile 403'ü ayırmak için önce kayıt yüklenmelidir.
    public class ProjeYetkiServisi
    {
        private readonly ApplicationDbContext _db;

        public ProjeYetkiServisi(ApplicationDbContext db)
        {
            _db = db;
        }

        public IQueryable<Proje> UyesiOldugumProjeler(string kullaniciId)
        {
            return _db.Projeler.Where(p => p.ProjeUyeleri.Any(u => u.KullaniciId == kullaniciId));
        }

        public Task<bool> UyeMiAsync(int projeId, string kullaniciId)
        {
            return _db.ProjeUyeleri.AnyAsync(u => u.ProjeId == projeId && u.KullaniciId == kullaniciId);
        }

        public Task<bool> OlusturanMiAsync(int projeId, string kullaniciId)
        {
            return _db.Projeler.AnyAsync(p => p.Id == projeId && p.OlusturanKullaniciId == kullaniciId);
        }
    }
}
