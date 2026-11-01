using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using AgileHub.Data;
using AgileHub.Models;

namespace AgileHub.Controllers;

public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public AccountController(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View(new KayitViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(KayitViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var email = model.Email.Trim();

        // Kullanıcı adı olarak e-posta kullanılıyor (bkz. SeedData).
        var kullanici = new ApplicationUser
        {
            UserName = email,
            Email = email,
            Ad = model.Ad.Trim(),
            Soyad = model.Soyad.Trim()
        };

        var sonuc = await _userManager.CreateAsync(kullanici, model.Sifre);

        if (!sonuc.Succeeded)
        {
            foreach (var hata in sonuc.Errors)
            {
                ModelState.AddModelError(string.Empty, hata.Description);
            }

            return View(model);
        }

        await _userManager.AddToRoleAsync(kullanici, Roller.Kullanici);
        await _signInManager.SignInAsync(kullanici, isPersistent: false);

        return RedirectToAction("Index", "Home");
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

    // Çıkış durum değiştirdiği için yalnızca POST kabul edilir (bağlantıyla çıkış yaptırılamaz).
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }
}
