using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using AgileHub.Models;

namespace AgileHub.Controllers;

public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AccountController(SignInManager<ApplicationUser> signInManager)
    {
        _signInManager = signInManager;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View(new GirisViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(GirisViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Kullanıcı adı olarak e-posta kullanılıyor (bkz. SeedData).
        var sonuc = await _signInManager.PasswordSignInAsync(
            model.Email, model.Sifre, model.BeniHatirla, lockoutOnFailure: true);

        if (sonuc.Succeeded)
        {
            // Başka bir siteye yönlendirmeyi önlemek için yalnızca yerel adresleri kabul ediyoruz.
            if (Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        var hata = sonuc.IsLockedOut
            ? "Çok fazla hatalı deneme yapıldı. Lütfen birkaç dakika sonra tekrar deneyin."
            : "E-posta veya şifre hatalı.";
        ModelState.AddModelError(string.Empty, hata);

        return View(model);
    }
}
