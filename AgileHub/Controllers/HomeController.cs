using System.Diagnostics;
using AgileHub.Models;
using AgileHub.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AgileHub.Controllers;

public class HomeController : Controller
{
    private readonly AnaSayfaServisi _anaSayfaServisi;
    private readonly UserManager<ApplicationUser> _userManager;

    public HomeController(AnaSayfaServisi anaSayfaServisi, UserManager<ApplicationUser> userManager)
    {
        _anaSayfaServisi = anaSayfaServisi;
        _userManager = userManager;
    }

    // GET: /
    // Giriş yapmamış ziyaretçiye tanıtım sayfası, giriş yapmış kullanıcıya pano gösterilir.
    public async Task<IActionResult> Index()
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return View("Tanitim");
        }

        var kullaniciId = _userManager.GetUserId(User)!;
        var model = await _anaSayfaServisi.PanoyuGetirAsync(kullaniciId);
        return View(model);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
