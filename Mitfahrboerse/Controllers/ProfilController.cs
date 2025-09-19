using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Mitfahrboerse.Models;
using Mitfahrboerse.Interfaces;
using Azure.Core;

namespace Mitfahrboerse.Controllers;

public class ProfilController : Controller
{
    private readonly MitfahrboerseDbContext _context;
    private readonly IAccessToken _accessToken;
    public ProfilController(MitfahrboerseDbContext context, IAccessToken acessToken)
    {
        _context = context;
        _accessToken = acessToken;
    }
    public async Task<IActionResult> Index()
    {
        string[] scopes = { "User.Read", "profile" };

        var accessToken = await _accessToken.GetAccessTokenAsync(scopes);
        ViewData["Token"] = accessToken;

        var client = await _accessToken.GetAuthorizedClientAsync(scopes);

        var photoresponse = await client.GetAsync("https://graph.microsoft.com/v1.0/me/photo/$value");
        var imageData = await photoresponse.Content.ReadAsByteArrayAsync();
        string base64 = Convert.ToBase64String(imageData);
        ViewData["ProfilePicture"] = $"data:image/jpeg;base64,{base64}";

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