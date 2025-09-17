using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Mitfahrboerse.Models;

namespace Mitfahrboerse.Controllers;

public class ProfilController : Controller
{
    private readonly MitfahrboerseDbContext _context;
    public ProfilController(MitfahrboerseDbContext context)
    {
        _context = context;
    }
    public IActionResult Index()
    {
        return View();   
    }

    public IActionResult Logout()
    {
        return SignOut(
            new AuthenticationProperties { RedirectUri = Url.Action("Index", "Profil") },
            OpenIdConnectDefaults.AuthenticationScheme,
            CookieAuthenticationDefaults.AuthenticationScheme);
    }
}