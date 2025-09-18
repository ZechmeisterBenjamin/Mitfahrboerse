using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices.JavaScript;
using System.Text.Json.Serialization;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Mvc;
using Mitfahrboerse.Models;
using RestSharp;
using Microsoft.Identity.Web;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Mitfahrboerse.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private IHttpClientFactory _httpClientFactory;
    private readonly ITokenAcquisition _tokenAcquisition;

    public HomeController(ILogger<HomeController> logger, IHttpClientFactory httpClientFactory, ITokenAcquisition tokenAcquisition)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _tokenAcquisition = tokenAcquisition;
    }

    public IActionResult Login()
    {
        return Challenge(
            new AuthenticationProperties
            {
                RedirectUri = Url.Action("Index", "Home")
            },
            OpenIdConnectDefaults.AuthenticationScheme);
    }
    [Authorize]
    public async Task<IActionResult> Index(string code)
    {
        try
        {
            string[] scopes = { "User.Read", "profile" };
            var accessToken = await _tokenAcquisition.GetAccessTokenForUserAsync(scopes);
            ViewData["Token"] = accessToken;

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await client.GetAsync("https://graph.microsoft.com/v1.0/me");
            var content = await response.Content.ReadAsStringAsync();

            

            ViewData["GraphResult"] = content;
            return View();
        }
        catch (MicrosoftIdentityWebChallengeUserException)
        {
            return Challenge(
                new AuthenticationProperties
                {
                    RedirectUri = Url.Action("Index", "Home")
                },
                OpenIdConnectDefaults.AuthenticationScheme);
        }
    }

    public IActionResult Logout()
    {
        return SignOut(
            new AuthenticationProperties { RedirectUri = Url.Action("Index", "Home") },
            OpenIdConnectDefaults.AuthenticationScheme,
            CookieAuthenticationDefaults.AuthenticationScheme);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}