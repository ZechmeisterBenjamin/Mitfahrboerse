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
        var user = await _context.t_People
            .Include(p => p.t_Cars)
            .Include(p => p.PersonOffers)
                .ThenInclude(po => po.FK_Offer)
            .Include(p => p.t_Rides)
                .ThenInclude(r => r.FK_StartsAt_Position)
            .Include(p => p.t_Rides)
                .ThenInclude(r => r.FK_EndsAt_Position)
            .Include(p => p.t_Rides)
                .ThenInclude(r => r.PersonRides)
            .Include(p => p.PersonRides)
                .ThenInclude(pr => pr.Ride)
                    .ThenInclude(r => r.FK_StartsAt_Position)
            .Include(p => p.PersonRides)
                .ThenInclude(pr => pr.Ride)
                    .ThenInclude(r => r.FK_EndsAt_Position)
            .FirstOrDefaultAsync(p => p.PersonId == personId);

        if (user == null)
        {
            user = new t_Person { PersonId = personId };
        }

        return View(user);
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
