using AgileHub.Data;
using AgileHub.Models;
using AgileHub.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AgileHub.Controllers
{
    // Kurallar GorevServisi'nde; controller yalnızca sonucu sayfaya çevirir.
    [Authorize]
    public class GorevController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly GorevServisi _gorevServisi;

        public GorevController(
            ApplicationDbContext db,
            UserManager<ApplicationUser> userManager,
            GorevServisi gorevServisi)
        {
            _db = db;
            _userManager = userManager;
            _gorevServisi = gorevServisi;
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

        // Proje üyelerinden açılır liste seçenekleri; adı girilmemiş kullanıcıda e-posta gösterilir.
        private static List<SelectListItem> KisiSecenekleri(IEnumerable<ApplicationUser> kisiler, string? seciliId)
        {
            return kisiler
                .Select(k => new SelectListItem
                {
                    Value = k.Id,
                    Text = string.IsNullOrWhiteSpace($"{k.Ad}{k.Soyad}") ? k.Email ?? k.Id : $"{k.Ad} {k.Soyad}".Trim(),
                    Selected = k.Id == seciliId
                })
                .ToList();
        }

        // GET: /Gorev?projeId=5  (tumu=true ile sprinttekiler de listelenir)
        public async Task<IActionResult> Index(int projeId, bool tumu = false, GorevDurumu? durum = null, string? atanan = null)
        {
            // Proje ve üyelik kontrolü bu çağrıda yapılır; filtre listesi de buradan gelir.
            var kisiler = await _gorevServisi.AtanabilecekKisileriGetirAsync(projeId, KullaniciId);

            if (!kisiler.BasariliMi)
            {
                return HataSonucu(kisiler.Durum);
            }

            var sonuc = await _gorevServisi.ProjeGorevleriniGetirAsync(
                projeId, KullaniciId, sadeceBacklog: !tumu, durum: durum, atananKullaniciId: atanan);

            if (!sonuc.BasariliMi)
            {
                return HataSonucu(sonuc.Durum);
            }

            var model = new GorevListesiViewModel
            {
                ProjeId = projeId,
                ProjeAdi = await _db.Projeler.Where(p => p.Id == projeId).Select(p => p.Ad).FirstAsync(),
                SadeceBacklog = !tumu,
                Durum = durum,
                AtananKullaniciId = atanan,
                Gorevler = sonuc.Deger!,
                AtanabilecekKisiler = KisiSecenekleri(kisiler.Deger!, atanan)
            };

            return View(model);
        }

        // Formdaki "Atanan kişi" listesini doldurur; proje yoksa ya da kullanıcı üye değilse hata sonucu döner.
        private async Task<IActionResult?> KisileriDoldurAsync(GorevFormViewModel model)
        {
            var kisiler = await _gorevServisi.AtanabilecekKisileriGetirAsync(model.ProjeId, KullaniciId);

            if (!kisiler.BasariliMi)
            {
                return HataSonucu(kisiler.Durum);
            }

            model.AtanabilecekKisiler = KisiSecenekleri(kisiler.Deger!, model.AtananKullaniciId);
            return null;
        }

        // GET: /Gorev/Create?projeId=5
        public async Task<IActionResult> Create(int projeId)
        {
            var model = new GorevFormViewModel { ProjeId = projeId };

            if (await KisileriDoldurAsync(model) is IActionResult hata)
            {
                return hata;
            }

            return View(model);
        }

        // POST: /Gorev/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GorevFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return await KisileriDoldurAsync(model) ?? View(model);
            }

            var sonuc = await _gorevServisi.OlusturAsync(
                model.ProjeId, model.Baslik, model.Aciklama, model.Oncelik, model.StoryPoint,
                model.AtananKullaniciId, KullaniciId);

            if (sonuc.Durum == IslemDurumu.Gecersiz)
            {
                ModelState.AddModelError(string.Empty, sonuc.Hata!);
                return await KisileriDoldurAsync(model) ?? View(model);
            }

            if (!sonuc.BasariliMi)
            {
                return HataSonucu(sonuc.Durum);
            }

            TempData["Mesaj"] = "Görev backlog'a eklendi.";
            return RedirectToAction(nameof(Index), new { projeId = model.ProjeId });
        }

        // GET: /Gorev/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var sonuc = await _gorevServisi.GetirAsync(id, KullaniciId);

            if (!sonuc.BasariliMi)
            {
                return HataSonucu(sonuc.Durum);
            }

            var gorev = sonuc.Deger!;
            var model = new GorevFormViewModel
            {
                Id = gorev.Id,
                ProjeId = gorev.ProjeId,
                Baslik = gorev.Baslik,
                Aciklama = gorev.Aciklama,
                Oncelik = gorev.Oncelik,
                Durum = gorev.Durum,
                StoryPoint = gorev.StoryPoint,
                AtananKullaniciId = gorev.AtananKullaniciId
            };

            if (await KisileriDoldurAsync(model) is IActionResult hata)
            {
                return hata;
            }

            // Projeden çıkarılan kişi listede yoktur; seçim sessizce "Atanmamış"a düşmesin diye uyarı gösterilir.
            if (gorev.AtananKullanici is not null && model.AtanabilecekKisiler.All(k => k.Value != gorev.AtananKullaniciId))
            {
                model.AtananKullaniciId = null;
                model.UyeOlmayanAtananAdi = $"{gorev.AtananKullanici.Ad} {gorev.AtananKullanici.Soyad}".Trim();
            }

            return View(model);
        }

        // POST: /Gorev/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, GorevFormViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            // Formdaki gizli ProjeId'ye güvenilmez; görevin gerçek projesi kullanılır.
            var projeId = await _db.Gorevler
                .Where(g => g.Id == id)
                .Select(g => (int?)g.ProjeId)
                .FirstOrDefaultAsync();

            if (projeId is null)
            {
                return NotFound();
            }

            model.ProjeId = projeId.Value;

            if (!ModelState.IsValid)
            {
                return await KisileriDoldurAsync(model) ?? View(model);
            }

            var sonuc = await _gorevServisi.GuncelleAsync(
                id, model.Baslik, model.Aciklama, model.Oncelik, model.Durum, model.StoryPoint,
                model.AtananKullaniciId, KullaniciId);

            if (sonuc.Durum == IslemDurumu.Gecersiz)
            {
                ModelState.AddModelError(string.Empty, sonuc.Hata!);
                return await KisileriDoldurAsync(model) ?? View(model);
            }

            if (!sonuc.BasariliMi)
            {
                return HataSonucu(sonuc.Durum);
            }

            TempData["Mesaj"] = "Görev güncellendi.";
            return RedirectToAction(nameof(Index), new { projeId = model.ProjeId });
        }
    }
}
