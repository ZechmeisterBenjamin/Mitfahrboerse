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
using Mitfahrboerse.Interfaces;

namespace Mitfahrboerse.Controllers;

public class HomeController : BaseController
{
    private readonly MitfahrboerseDbContext _context;
    public HomeController(MitfahrboerseDbContext context, ILogger<HomeController> logger, IAccessToken accessToken) : base(logger, accessToken, context)
    {
        _context = context;
    }


    [Authorize]
    public IActionResult Index(string code)
    {
        return View();
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
    [Authorize]
    public IActionResult Start()
    {
        var person = _context.t_People.FirstOrDefault(p => p.PersonId == personId);

        if (person == null) 
        {
            return RedirectToAction("Index", "Ride");
        }

        switch (person.Startpage)
        {
            case 1:
                return RedirectToAction("Create", "Ride");
            default:
                return RedirectToAction("Index", "Ride");
        }
    }
}