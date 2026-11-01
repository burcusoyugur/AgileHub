using AgileHub.Data;
using AgileHub.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AgileHub.Controllers
{
    [Authorize]
    public class ProjeController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProjeController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        private string KullaniciId => _userManager.GetUserId(User)!;

        // Kullanıcının üyesi olduğu projeler
        private IQueryable<Proje> UyesiOldugumProjeler()
        {
            var kullaniciId = KullaniciId;
            return _db.Projeler.Where(p => p.ProjeUyeleri.Any(u => u.KullaniciId == kullaniciId));
        }

        // GET: /Proje
        public async Task<IActionResult> Index()
        {
            var projeler = await UyesiOldugumProjeler()
                .Include(p => p.OlusturanKullanici)
                .Include(p => p.ProjeUyeleri)
                .OrderByDescending(p => p.OlusturmaTarihi)
                .AsNoTracking()
                .ToListAsync();

            return View(projeler);
        }

        // GET: /Proje/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var proje = await _db.Projeler
                .Include(p => p.OlusturanKullanici)
                .Include(p => p.ProjeUyeleri).ThenInclude(u => u.Kullanici)
                .Include(p => p.Sprintler)
                .Include(p => p.Gorevler)
                .AsSplitQuery()
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);

            if (proje is null)
            {
                return NotFound();
            }

            if (!proje.ProjeUyeleri.Any(u => u.KullaniciId == KullaniciId))
            {
                return Forbid();
            }

            ViewBag.OlusturanBenMiyim = proje.OlusturanKullaniciId == KullaniciId;
            return View(proje);
        }

        // GET: /Proje/Create
        public IActionResult Create()
        {
            return View(new ProjeFormViewModel());
        }

        // POST: /Proje/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProjeFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var proje = new Proje
            {
                Ad = model.Ad.Trim(),
                Aciklama = model.Aciklama,
                OlusturmaTarihi = DateTime.Now,
                OlusturanKullaniciId = KullaniciId
            };

            // Projeyi oluşturan kişi aynı zamanda projenin ilk üyesidir.
            proje.ProjeUyeleri.Add(new ProjeUyesi
            {
                KullaniciId = KullaniciId,
                KatilmaTarihi = DateTime.Now
            });

            _db.Projeler.Add(proje);
            await _db.SaveChangesAsync();

            TempData["Mesaj"] = "Proje oluşturuldu.";
            return RedirectToAction(nameof(Details), new { id = proje.Id });
        }

        // GET: /Proje/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var proje = await _db.Projeler.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);

            if (proje is null)
            {
                return NotFound();
            }

            if (proje.OlusturanKullaniciId != KullaniciId)
            {
                return Forbid();
            }

            return View(new ProjeFormViewModel
            {
                Id = proje.Id,
                Ad = proje.Ad,
                Aciklama = proje.Aciklama
            });
        }

        // POST: /Proje/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProjeFormViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            var proje = await _db.Projeler.FirstOrDefaultAsync(p => p.Id == id);

            if (proje is null)
            {
                return NotFound();
            }

            if (proje.OlusturanKullaniciId != KullaniciId)
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            proje.Ad = model.Ad.Trim();
            proje.Aciklama = model.Aciklama;
            await _db.SaveChangesAsync();

            TempData["Mesaj"] = "Proje güncellendi.";
            return RedirectToAction(nameof(Details), new { id = proje.Id });
        }

        // GET: /Proje/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var proje = await _db.Projeler
                .Include(p => p.OlusturanKullanici)
                .Include(p => p.Sprintler)
                .Include(p => p.Gorevler)
                .Include(p => p.ProjeUyeleri)
                .AsSplitQuery()
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);

            if (proje is null)
            {
                return NotFound();
            }

            if (proje.OlusturanKullaniciId != KullaniciId)
            {
                return Forbid();
            }

            return View(proje);
        }

        // POST: /Proje/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var proje = await _db.Projeler
                .Include(p => p.Sprintler)
                .Include(p => p.Gorevler)
                .AsSplitQuery()
                .FirstOrDefaultAsync(p => p.Id == id);

            if (proje is null)
            {
                return NotFound();
            }

            if (proje.OlusturanKullaniciId != KullaniciId)
            {
                return Forbid();
            }

            // Gorev -> Sprint ilişkisi NoAction olduğu için görevleri ve sprintleri
            // açıkça siliyoruz; EF önce görevleri, sonra sprintleri siler.
            // Yorumlar, görev etiketleri ve proje üyeleri veritabanında cascade ile silinir.
            _db.Gorevler.RemoveRange(proje.Gorevler);
            _db.Sprintler.RemoveRange(proje.Sprintler);
            _db.Projeler.Remove(proje);
            await _db.SaveChangesAsync();

            TempData["Mesaj"] = "Proje silindi.";
            return RedirectToAction(nameof(Index));
        }
    }
}
