using AgileHub.Models;
using AgileHub.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AgileHub.Controllers
{
    // Görev detayındaki yorum formlarının hedefi. Kurallar YorumServisi'nde;
    // controller sonucu yönlendirmeye ya da 403/404'e çevirir.
    [Authorize]
    public class YorumController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly YorumServisi _yorumServisi;

        public YorumController(UserManager<ApplicationUser> userManager, YorumServisi yorumServisi)
        {
            _userManager = userManager;
            _yorumServisi = yorumServisi;
        }

        private string KullaniciId => _userManager.GetUserId(User)!;

        private IActionResult HataSonucu(IslemDurumu durum)
        {
            return durum switch
            {
                IslemDurumu.Bulunamadi => NotFound(),
                IslemDurumu.Yetkisiz => Forbid(),
                _ => BadRequest()
            };
        }

        // Yorum bölümüne (id="yorumlar") dönülür.
        private IActionResult GoreveDon(int gorevId)
        {
            return RedirectToAction("Details", "Gorev", new { id = gorevId }, "yorumlar");
        }

        // POST: /Yorum/Ekle
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Ekle(int gorevId, string? metin)
        {
            var sonuc = await _yorumServisi.EkleAsync(gorevId, metin, KullaniciId);

            if (sonuc.Durum == IslemDurumu.Gecersiz)
            {
                TempData["Hata"] = sonuc.Hata;
                return GoreveDon(gorevId);
            }

            if (!sonuc.BasariliMi)
            {
                return HataSonucu(sonuc.Durum);
            }

            TempData["Mesaj"] = "Yorum eklendi.";
            return GoreveDon(gorevId);
        }

        // POST: /Yorum/Sil/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Sil(int id)
        {
            var sonuc = await _yorumServisi.SilAsync(id, KullaniciId);

            if (!sonuc.BasariliMi)
            {
                return HataSonucu(sonuc.Durum);
            }

            TempData["Mesaj"] = "Yorum silindi.";
            return GoreveDon(sonuc.Deger);
        }
    }
}
