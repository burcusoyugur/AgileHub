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

        public SprintController(
            ApplicationDbContext db,
            UserManager<ApplicationUser> userManager,
            SprintServisi sprintServisi)
        {
            _db = db;
            _userManager = userManager;
            _sprintServisi = sprintServisi;
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
    }
}
