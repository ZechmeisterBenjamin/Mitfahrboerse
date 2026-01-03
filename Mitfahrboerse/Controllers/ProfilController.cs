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
        LoadRideHistory();
        return View();   
    }

    private void LoadRideHistory()
    {
        List<t_Ride> rides = _context.t_Rides.Where(r => r.Status == 1 && r.IsProcessed == false && r.FK_Driver_PersonId == personId).ToList();
        List<t_PersonRide> joinedRides = _context.t_PersonRides.Where(r => r.Status == 1 && r.IsProcessed == false && r.FK_PersonId == personId).ToList();
        double distance = 0.0;
        foreach (var ride in rides)
        {
            distance += ride.Distance;
        }
        
        ViewData["RidesSum"] = rides.Count();
        ViewData["Distance"] = distance;

    ViewData["JoinedRidesSum"] = joinedRides.Count();
    ViewData["DistinctPassengersSum"] = _context.t_PersonRides
        .Where(p => p.Status == 1 && p.IsProcessed == false && p.Ride.Status == 1 && p.Ride.IsProcessed == false && p.Ride.FK_Driver_PersonId == personId)
        .Select(p => p.FK_PersonId)
        .Distinct()
        .Count();    
    }

    [HttpPost]
    public IActionResult CreateCar(string kennzeichen, short sitze, string marke, string modell, string farbe)
    {
        var newCar = new t_Car(
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