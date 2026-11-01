using Microsoft.AspNetCore.Mvc;
using AgileHub.Models;

namespace AgileHub.Controllers;

public class AccountController : Controller
{
    [HttpGet]
    public IActionResult Login()
    {
        return View(new GirisViewModel());
    }
}
