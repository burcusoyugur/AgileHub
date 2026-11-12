using System.Security.Claims;
using AgileHub.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgileHub.Controllers.Api
{
    // Mobil ve Swagger için API uçlarının ortak tabanı.
    // Kimlik JWT ile doğrulanır; token yoksa veya geçersizse 401 döner (web'deki gibi giriş sayfasına yönlendirmez).
    // İş kuralları ve yetki servislerdedir; controller'lar servis sonucunu HTTP yanıtına çevirir.
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Produces("application/json")]
    public abstract class ApiTemelController : ControllerBase
    {
        // JwtBearer, token'daki "sub" claim'ini NameIdentifier'a eşler.
        protected string KullaniciId =>
            User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")
            ?? throw new InvalidOperationException("Token'da kullanıcı kimliği yok.");

        // Başarısız servis sonucunu ProblemDetails gövdeli yanıta çevirir.
        // Forbid() kullanılmaz: varsayılan şema cookie olduğu için 403 yerine
        // /Account/AccessDenied sayfasına yönlendirirdi.
        protected ActionResult HataSonucu(IslemDurumu durum, string? hata)
        {
            return durum switch
            {
                IslemDurumu.Bulunamadi => Problem(statusCode: StatusCodes.Status404NotFound, title: "Kayıt bulunamadı"),
                IslemDurumu.Yetkisiz => Problem(statusCode: StatusCodes.Status403Forbidden, title: "Bu kayda erişim yetkin yok"),
                IslemDurumu.Gecersiz => Problem(statusCode: StatusCodes.Status400BadRequest, title: "Geçersiz istek", detail: hata),
                _ => throw new ArgumentOutOfRangeException(nameof(durum), durum, "Başarılı sonuç hata yanıtına çevrilemez.")
            };
        }

        protected ActionResult HataSonucu(IslemSonucu sonuc) => HataSonucu(sonuc.Durum, sonuc.Hata);

        protected ActionResult HataSonucu<T>(IslemSonucu<T> sonuc) => HataSonucu(sonuc.Durum, sonuc.Hata);
    }
}
