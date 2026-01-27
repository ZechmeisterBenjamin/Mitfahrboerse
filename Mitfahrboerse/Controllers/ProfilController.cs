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

    public ProfilController(MitfahrboerseDbContext context, ILogger<ProfilController> logger, IAccessToken accessToken)
        : base(logger, accessToken, context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        LoadRideHistory();

        var person = _context.t_People.FirstOrDefault(p => p.PersonId == personId);
        ViewData["SelectedDesign"] = person?.Design ?? 0;
        ViewData["SelectedStartseite"] = person?.Startpage ?? 0;
        ViewData["Cars"] = _context.t_Cars
            .Where(c => c.FK_Owner_PersonId == personId)
            .ToList();

        return View();
    }

    private void LoadRideHistory()
    {
        List<t_Ride> rides = _context.t_Rides.Where(r => r.Status == 1 && r.FK_Driver_PersonId == personId).ToList();
        List<t_PersonRide> joinedRides =
            _context.t_PersonRides.Where(r => r.Status == 1 && r.FK_PersonId == personId).ToList();
        double distance = rides.Sum(r => r.Distance);

        ViewData["RidesSum"] = rides.Count();
        ViewData["Distance"] = distance;

        ViewData["JoinedRidesSum"] = joinedRides.Count();
        ViewData["DistinctPassengersSum"] = _context.t_PersonRides
            .Where(p => p.Status == 1 && p.Ride != null && p.Ride.Status == 1 && p.Ride.FK_Driver_PersonId == personId)
            .Select(p => p.FK_PersonId)
            .Distinct()
            .Count();
    }

    [HttpPost]
    public IActionResult UpdateSettings([FromBody] SettingsUpdateModel model)
    {
        var person = _context.t_People.FirstOrDefault(p => p.PersonId == personId);
        if (person == null)
        {
            return NotFound();
        }

        person.Design = (byte)model.SelectedDesign;
        person.Startpage = (byte)model.SelectedStartseite;
        _context.SaveChanges();

        return Ok(new { success = true });
    }

    public class SettingsUpdateModel
    {
        public int SelectedDesign { get; set; }
        public int SelectedStartseite { get; set; }
    }
    [HttpPost]
    public IActionResult CreateCar(string kennzeichen, short sitze, string marke, string modell, string farbe)
    {
        int nextId = 1;
        if (_context.t_Cars.Any())
        {
            nextId = _context.t_Cars.Max(c => c.CarId) + 1;
        }

        var newCar = new t_Car(
            nextId,
            kennzeichen ?? "",
            sitze,
            marke ?? "",
            modell ?? "",
            farbe ?? "",
            personId
        );
        _context.t_Cars.Add(newCar);
        _context.SaveChanges();

        return RedirectToAction("Index");
    }

    [HttpPost]
    public IActionResult DeleteCar(int carId)
    {
        var car = _context.t_Cars.FirstOrDefault(c => c.CarId == carId && c.FK_Owner_PersonId == personId);
        if (car == null)
        {
            TempData["CarDeleteError"] = "Auto nicht gefunden.";
            return RedirectToAction("Index");
        }

        if (_context.t_Rides.Any(r => r.FK_CarId == car.CarId))
        {
            TempData["CarDeleteError"] = "Auto wird in einer Fahrt verwendet.";
            return RedirectToAction("Index");
        }

        _context.t_Cars.Remove(car);
        _context.SaveChanges();

        TempData["CarDeleteSuccess"] = "Auto entfernt.";
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
