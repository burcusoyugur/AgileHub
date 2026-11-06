using AgileHub.Data;
using AgileHub.Models;
using AgileHub.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AgileHub.Controllers
{
    // Kurallar SprintServisi'nde; controller yalnızca sonucu sayfaya çevirir.
    [Authorize]
    public class SprintController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SprintServisi _sprintServisi;
        private readonly ProjeYetkiServisi _yetki;

        public SprintController(
            ApplicationDbContext db,
            UserManager<ApplicationUser> userManager,
            SprintServisi sprintServisi,
            ProjeYetkiServisi yetki)
        {
            _db = db;
            _userManager = userManager;
            _sprintServisi = sprintServisi;
            _yetki = yetki;
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

        // GET: /Sprint?projeId=5
        public async Task<IActionResult> Index(int projeId)
        {
            var sonuc = await _sprintServisi.ProjeSprintleriniGetirAsync(projeId, KullaniciId);

            if (!sonuc.BasariliMi)
            {
                return HataSonucu(sonuc.Durum);
            }

            ViewBag.ProjeId = projeId;
            ViewBag.ProjeAdi = await _db.Projeler
                .Where(p => p.Id == projeId)
                .Select(p => p.Ad)
                .FirstAsync();

            return View(sonuc.Deger);
        }

        // GET: /Sprint/Create?projeId=5
        public async Task<IActionResult> Create(int projeId)
        {
            if (!await _db.Projeler.AnyAsync(p => p.Id == projeId))
            {
                return NotFound();
            }

            if (!await _yetki.UyeMiAsync(projeId, KullaniciId))
            {
                return Forbid();
            }

            return View(new SprintFormViewModel { ProjeId = projeId });
        }

        // POST: /Sprint/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SprintFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var sonuc = await _sprintServisi.OlusturAsync(
                model.ProjeId, model.Ad, model.Hedef, model.BaslangicTarihi, model.BitisTarihi, KullaniciId);

            if (sonuc.Durum == IslemDurumu.Gecersiz)
            {
                ModelState.AddModelError(string.Empty, sonuc.Hata!);
                return View(model);
            }

            if (!sonuc.BasariliMi)
            {
                return HataSonucu(sonuc.Durum);
            }

            TempData["Mesaj"] = "Sprint oluşturuldu.";
            return RedirectToAction(nameof(Index), new { projeId = model.ProjeId });
        }

        // GET: /Sprint/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var sonuc = await _sprintServisi.GetirAsync(id, KullaniciId);

            if (!sonuc.BasariliMi)
            {
                return HataSonucu(sonuc.Durum);
            }

            var sprint = sonuc.Deger!;

            if (sprint.Durum is SprintDurumu.Tamamlandı or SprintDurumu.İptalEdildi)
            {
                TempData["Hata"] = "Tamamlanmış veya iptal edilmiş sprint düzenlenemez.";
                return RedirectToAction(nameof(Index), new { projeId = sprint.ProjeId });
            }

            return View(new SprintFormViewModel
            {
                Id = sprint.Id,
                ProjeId = sprint.ProjeId,
                Ad = sprint.Ad,
                Hedef = sprint.Hedef,
                BaslangicTarihi = sprint.BaslangicTarihi,
                BitisTarihi = sprint.BitisTarihi
            });
        }

        // POST: /Sprint/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SprintFormViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var sonuc = await _sprintServisi.GuncelleAsync(
                id, model.Ad, model.Hedef, model.BaslangicTarihi, model.BitisTarihi, KullaniciId);

            if (sonuc.Durum == IslemDurumu.Gecersiz)
            {
                ModelState.AddModelError(string.Empty, sonuc.Hata!);
                return View(model);
            }

            if (!sonuc.BasariliMi)
            {
                return HataSonucu(sonuc.Durum);
            }

            TempData["Mesaj"] = "Sprint güncellendi.";
            return RedirectToAction(nameof(Index), new { projeId = model.ProjeId });
        }
    }
}
