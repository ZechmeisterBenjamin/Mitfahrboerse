using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Mitfahrboerse.Models;
using Mitfahrboerse.Interfaces;
using Azure.Core;

namespace Mitfahrboerse.Controllers;

public class ProfilController : BaseController
{
    private readonly MitfahrboerseDbContext _context;
    public ProfilController(MitfahrboerseDbContext context, ILogger<ProfilController> logger, IAccessToken accessToken) : base(logger, accessToken, context)
    {
        _context = context;
    }
    public async Task<IActionResult> Index()
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