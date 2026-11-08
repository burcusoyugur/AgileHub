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
    }
}
