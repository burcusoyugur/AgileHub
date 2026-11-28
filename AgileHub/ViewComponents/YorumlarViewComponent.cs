using AgileHub.Models;
using AgileHub.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AgileHub.ViewComponents
{
    // Görev detayındaki yorum bölümü. Yorumları servisten kendisi alır;
    // böylece detay sayfasının controller'ı değişmeden tek satırla eklenir:
    // @await Component.InvokeAsync("Yorumlar", new { gorevId = Model.Id })
    public class YorumlarViewComponent : ViewComponent
    {
        private readonly YorumServisi _yorumServisi;
        private readonly UserManager<ApplicationUser> _userManager;

        public YorumlarViewComponent(YorumServisi yorumServisi, UserManager<ApplicationUser> userManager)
        {
            _yorumServisi = yorumServisi;
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync(int gorevId)
        {
            var kullaniciId = _userManager.GetUserId(UserClaimsPrincipal) ?? string.Empty;
            var sonuc = await _yorumServisi.GorevYorumlariniGetirAsync(gorevId, kullaniciId);

            // Detay sayfası erişimi zaten denetliyor; buraya düşülürse bölüm boş kalır.
            if (!sonuc.BasariliMi)
            {
                return Content(string.Empty);
            }

            return View(new YorumlarBileseniModeli(gorevId, kullaniciId, sonuc.Deger!));
        }
    }

    public record YorumlarBileseniModeli(int GorevId, string KullaniciId, List<Yorum> Yorumlar);
}
