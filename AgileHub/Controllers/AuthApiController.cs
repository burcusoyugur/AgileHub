using AgileHub.Data;
using AgileHub.Models;
using AgileHub.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AgileHub.Controllers
{
    /// <summary>Mobil uygulamanın giriş, kayıt ve oturum kontrolü uçları.</summary>
    [ApiController]
    [Route("api/auth")]
    [Produces("application/json")]
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

        /// <summary>E-posta ve şifreyle giriş yapar; başarılıysa JWT token döner.</summary>
        /// <remarks>
        /// Yanıttaki <c>token</c> değeri korumalı uçlarda <c>Authorization: Bearer &lt;token&gt;</c> başlığıyla gönderilir.
        /// Art arda hatalı denemelerde hesap web girişindeki gibi geçici olarak kilitlenir.
        /// </remarks>
        /// <param name="istek">Giriş bilgileri.</param>
        /// <response code="200">Giriş başarılı; token, bitiş zamanı ve kullanıcı bilgisi.</response>
        /// <response code="400">E-posta veya şifre boş ya da e-posta biçimi geçersiz.</response>
        /// <response code="401">E-posta veya şifre hatalı ya da hesap kilitli.</response>
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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

        /// <summary>Yeni hesap oluşturur ve hemen giriş yapmış olarak token döner.</summary>
        /// <remarks>Ad soyad tek alanda alınır; son kelime soyad olarak kaydedilir. Yeni hesap Kullanici rolüne atanır.</remarks>
        /// <param name="istek">Ad soyad, e-posta ve şifre.</param>
        /// <response code="200">Hesap oluşturuldu; token, bitiş zamanı ve kullanıcı bilgisi.</response>
        /// <response code="400">Eksik veya geçersiz bilgi ya da şifre kurallara uymuyor (ayrıntı <c>errors</c> alanında).</response>
        /// <response code="409">Bu e-posta ile kayıtlı bir hesap zaten var.</response>
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
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

        /// <summary>Token'ın sahibi olan kullanıcının bilgilerini döner.</summary>
        /// <remarks>Mobil uygulama açılışta kayıtlı token'ın hâlâ geçerli olup olmadığını bu uçla anlar.</remarks>
        /// <response code="200">Giriş yapmış kullanıcı.</response>
        /// <response code="401">Token yok, geçersiz veya süresi dolmuş.</response>
        [HttpGet("me")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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
