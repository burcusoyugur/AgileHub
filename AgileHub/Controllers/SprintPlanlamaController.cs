using AgileHub.Models;
using AgileHub.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AgileHub.Controllers
{
    // Kurallar SprintAtamaServisi'nde; controller yalnızca sonucu sayfaya çevirir.
    [Authorize]
    public class SprintPlanlamaController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SprintAtamaServisi _atamaServisi;

        public SprintPlanlamaController(
            UserManager<ApplicationUser> userManager,
            SprintAtamaServisi atamaServisi)
        {
            _userManager = userManager;
            _atamaServisi = atamaServisi;
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

        private IActionResult PlanlamayaDon(int projeId, int sprintId)
        {
            return RedirectToAction(nameof(Index), new { projeId, sprintId });
        }

        // GET: /SprintPlanlama?projeId=5&sprintId=2
        public async Task<IActionResult> Index(int projeId, int? sprintId)
        {
            var sonuc = await _atamaServisi.PlanlamaVerisiniGetirAsync(projeId, sprintId, KullaniciId);

            if (!sonuc.BasariliMi)
            {
                return HataSonucu(sonuc.Durum);
            }

            return View(sonuc.Deger);
        }

        // POST: /SprintPlanlama/SpriteAta
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SpriteAta(int projeId, int sprintId, int gorevId)
        {
            var sonuc = await _atamaServisi.SpriteAtaAsync(gorevId, sprintId, KullaniciId);

            if (sonuc.Durum == IslemDurumu.Gecersiz)
            {
                TempData["Hata"] = sonuc.Hata;
                return PlanlamayaDon(projeId, sprintId);
            }

            if (!sonuc.BasariliMi)
            {
                return HataSonucu(sonuc.Durum);
            }

            TempData["Mesaj"] = "Görev sprinte eklendi.";
            return PlanlamayaDon(projeId, sprintId);
        }

        // POST: /SprintPlanlama/SprinttenCikar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SprinttenCikar(int projeId, int sprintId, int gorevId)
        {
            var sonuc = await _atamaServisi.SprinttenCikarAsync(gorevId, KullaniciId);

            if (sonuc.Durum == IslemDurumu.Gecersiz)
            {
                TempData["Hata"] = sonuc.Hata;
                return PlanlamayaDon(projeId, sprintId);
            }

            if (!sonuc.BasariliMi)
            {
                return HataSonucu(sonuc.Durum);
            }

            TempData["Mesaj"] = "Görev backlog'a alındı.";
            return PlanlamayaDon(projeId, sprintId);
        }

        // POST: /SprintPlanlama/TopluAta
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TopluAta(int projeId, int sprintId, int[]? gorevIdleri)
        {
            var sonuc = await _atamaServisi.TopluAtaAsync(sprintId, gorevIdleri ?? [], KullaniciId);

            if (sonuc.Durum == IslemDurumu.Gecersiz)
            {
                TempData["Hata"] = sonuc.Hata;
                return PlanlamayaDon(projeId, sprintId);
            }

            if (!sonuc.BasariliMi)
            {
                return HataSonucu(sonuc.Durum);
            }

            TempData["Mesaj"] = $"{sonuc.Deger} görev sprinte eklendi.";
            return PlanlamayaDon(projeId, sprintId);
        }
    }
}
