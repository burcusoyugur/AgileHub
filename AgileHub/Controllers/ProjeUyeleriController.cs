using AgileHub.Data;
using AgileHub.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AgileHub.Controllers
{
    [Authorize]
    public class ProjeUyeleriController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProjeUyeleriController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        private string KullaniciId => _userManager.GetUserId(User)!;

        private IActionResult ProjeDetayinaDon(int projeId)
        {
            return RedirectToAction("Details", "Proje", new { id = projeId }, "uyeler");
        }

        // POST: /ProjeUyeleri/Ekle
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Ekle(int projeId, string? email)
        {
            var proje = await _db.Projeler.AsNoTracking().FirstOrDefaultAsync(p => p.Id == projeId);

            if (proje is null)
            {
                return NotFound();
            }

            if (proje.OlusturanKullaniciId != KullaniciId)
            {
                return Forbid();
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                TempData["Hata"] = "E-posta adresi zorunludur.";
                return ProjeDetayinaDon(projeId);
            }

            var kullanici = await _userManager.FindByEmailAsync(email.Trim());

            if (kullanici is null)
            {
                TempData["Hata"] = "Bu e-posta adresiyle kayıtlı kullanıcı bulunamadı.";
                return ProjeDetayinaDon(projeId);
            }

            var zatenUye = await _db.ProjeUyeleri
                .AnyAsync(u => u.ProjeId == projeId && u.KullaniciId == kullanici.Id);

            if (zatenUye)
            {
                TempData["Hata"] = "Bu kullanıcı zaten projenin üyesi.";
                return ProjeDetayinaDon(projeId);
            }

            _db.ProjeUyeleri.Add(new ProjeUyesi
            {
                ProjeId = projeId,
                KullaniciId = kullanici.Id,
                KatilmaTarihi = DateTime.Now
            });

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // Aynı anda iki istek gelirse benzersiz indeks ikincisini reddeder.
                TempData["Hata"] = "Bu kullanıcı zaten projenin üyesi.";
                return ProjeDetayinaDon(projeId);
            }

            TempData["Mesaj"] = $"{kullanici.Ad} {kullanici.Soyad} projeye eklendi.";
            return ProjeDetayinaDon(projeId);
        }

        // POST: /ProjeUyeleri/Cikar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cikar(int projeId, int uyeId)
        {
            var proje = await _db.Projeler.AsNoTracking().FirstOrDefaultAsync(p => p.Id == projeId);

            if (proje is null)
            {
                return NotFound();
            }

            if (proje.OlusturanKullaniciId != KullaniciId)
            {
                return Forbid();
            }

            var uye = await _db.ProjeUyeleri
                .Include(u => u.Kullanici)
                .FirstOrDefaultAsync(u => u.Id == uyeId && u.ProjeId == projeId);

            if (uye is null)
            {
                return NotFound();
            }

            if (uye.KullaniciId == proje.OlusturanKullaniciId)
            {
                TempData["Hata"] = "Projeyi oluşturan kişi projeden çıkarılamaz.";
                return ProjeDetayinaDon(projeId);
            }

            _db.ProjeUyeleri.Remove(uye);
            await _db.SaveChangesAsync();

            TempData["Mesaj"] = $"{uye.Kullanici?.Ad} {uye.Kullanici?.Soyad} projeden çıkarıldı.";
            return ProjeDetayinaDon(projeId);
        }
    }
}
