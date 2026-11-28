using AgileHub.Models;
using AgileHub.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AgileHub.Controllers
{
    // Kanban panosu. Yetki ve veri kuralları servislerde; controller yalnızca sayfayı hazırlar.
    // Kartların taşınması GorevController.DurumGuncelle üzerinden yapılır.
    [Authorize]
    public class PanoController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ProjeServisi _projeServisi;
        private readonly SprintServisi _sprintServisi;

        public PanoController(
            UserManager<ApplicationUser> userManager,
            ProjeServisi projeServisi,
            SprintServisi sprintServisi)
        {
            _userManager = userManager;
            _projeServisi = projeServisi;
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

        // GET: /Pano?projeId=5&sprintId=2  (sprintId yoksa aktif sprint gösterilir)
        public async Task<IActionResult> Index(int projeId, int? sprintId)
        {
            // Proje ve üyelik kontrolü bu çağrıda yapılır; proje adı da buradan gelir.
            var proje = await _projeServisi.GetirAsync(projeId, KullaniciId);

            if (!proje.BasariliMi)
            {
                return HataSonucu(proje.Durum);
            }

            var sprintler = await _sprintServisi.ProjeSprintleriniGetirAsync(projeId, KullaniciId);

            if (!sprintler.BasariliMi)
            {
                return HataSonucu(sprintler.Durum);
            }

            var secili = sprintId is int istenen
                ? sprintler.Deger!.FirstOrDefault(s => s.Id == istenen)
                : sprintler.Deger!.FirstOrDefault(s => s.Durum == SprintDurumu.Aktif);

            // Olmayan ya da başka projeye ait sprint.
            if (sprintId is not null && secili is null)
            {
                return NotFound();
            }

            var model = new PanoViewModel
            {
                ProjeId = projeId,
                ProjeAdi = proje.Deger!.Ad,
                Sprintler = sprintler.Deger!
            };

            if (secili is not null)
            {
                // Atanan kişilerin adları yalnızca bu sorguda yüklenir.
                var sprint = await _sprintServisi.GetirAsync(secili.Id, KullaniciId);

                if (!sprint.BasariliMi)
                {
                    return HataSonucu(sprint.Durum);
                }

                model.SeciliSprint = sprint.Deger;
                model.Sutunlar = PanoViewModel.SutunlaraAyir(sprint.Deger!.Gorevler);
            }

            return View(model);
        }
    }
}
