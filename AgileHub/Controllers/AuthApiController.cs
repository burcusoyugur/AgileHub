using AgileHub.Data;
using AgileHub.Models;
using AgileHub.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AgileHub.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthApiController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly JwtServisi _jwtServisi;

        public AuthApiController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            JwtServisi jwtServisi)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtServisi = jwtServisi;
        }

        // POST: /api/auth/login
        [HttpPost("login")]
        public async Task<ActionResult<AuthYaniti>> Login(GirisIstegi istek)
        {
            var kullanici = await _userManager.FindByEmailAsync(istek.Email.Trim());

            if (kullanici is null)
            {
                return Unauthorized();
            }

            // Web girişiyle aynı kilitleme kuralı geçerlidir; cookie oluşturulmaz.
            var sonuc = await _signInManager.CheckPasswordSignInAsync(
                kullanici, istek.Password, lockoutOnFailure: true);

            if (!sonuc.Succeeded)
            {
                return Unauthorized();
            }

            return _jwtServisi.YanitOlustur(kullanici);
        }

        // POST: /api/auth/register
        [HttpPost("register")]
        public async Task<ActionResult<AuthYaniti>> Register(KayitIstegi istek)
        {
            var email = istek.Email.Trim();

            if (await _userManager.FindByEmailAsync(email) is not null)
            {
                return Conflict();
            }

            // Mobilde ad soyad tek alanda alınır; son kelime soyad kabul edilir.
            var adSoyad = istek.FullName.Trim();
            var bosluk = adSoyad.LastIndexOf(' ');

            var kullanici = new ApplicationUser
            {
                UserName = email,
                Email = email,
                Ad = bosluk < 0 ? adSoyad : adSoyad[..bosluk].TrimEnd(),
                Soyad = bosluk < 0 ? string.Empty : adSoyad[(bosluk + 1)..]
            };

            var sonuc = await _userManager.CreateAsync(kullanici, istek.Password);

            if (!sonuc.Succeeded)
            {
                // Aynı anda iki istek gelirse ikincisi burada yakalanır.
                if (sonuc.Errors.Any(e => e.Code == nameof(IdentityErrorDescriber.DuplicateUserName)))
                {
                    return Conflict();
                }

                foreach (var hata in sonuc.Errors)
                {
                    ModelState.AddModelError(hata.Code, hata.Description);
                }

                return ValidationProblem(ModelState);
            }

            await _userManager.AddToRoleAsync(kullanici, Roller.Kullanici);

            return _jwtServisi.YanitOlustur(kullanici);
        }

        // GET: /api/auth/me
        [HttpGet("me")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<KullaniciDto>> Me()
        {
            var kullanici = await _userManager.GetUserAsync(User);

            if (kullanici is null)
            {
                return Unauthorized();
            }

            return new KullaniciDto(kullanici.Id, kullanici.Email!, kullanici.Ad, kullanici.Soyad);
        }
    }
}
