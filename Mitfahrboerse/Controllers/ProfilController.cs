using Azure.Core;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mitfahrboerse.Interfaces;
using Mitfahrboerse.Models;
using System.Security.Claims;

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
        var user = _context.t_People
            .Include(p => p.PersonOffers)
                .ThenInclude(po => po.FK_Offer)
            .FirstOrDefault(p => p.PersonId == personId);

        if (user == null)
        {
            user = new t_Person { PersonId = personId, PersonOffers = new List<t_PersonOffer>() };
        }

        return View(user);
    }
    [HttpPost]
    public IActionResult CreateCar(string kennzeichen, short sitze, string marke, string modell, string farbe)
    {
        int nextId = _context.t_Cars.Any() ? _context.t_Cars.Max(c => c.CarId) + 1 : 1;

        var newCar = new t_Car(
            nextId,
            kennzeichen ?? "", 
            sitze, 
            marke ?? "", 
            modell ?? "", 
            farbe ?? "", 
            personId
        );        _context.t_Cars.Add(newCar);
        _context.SaveChanges();
        
        return RedirectToAction("Index");
    }

    public IActionResult Logout()
    {
        return SignOut(
            new AuthenticationProperties { RedirectUri = Url.Action("Index", "Profil") },
            OpenIdConnectDefaults.AuthenticationScheme,
            CookieAuthenticationDefaults.AuthenticationScheme);
    }
}