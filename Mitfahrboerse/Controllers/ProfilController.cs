using Azure.Core;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mitfahrboerse.Interfaces;
using Mitfahrboerse.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;
using Mitfahrboerse.Hubs;

namespace Mitfahrboerse.Controllers;

public class ProfilController : BaseController
{
    private readonly MitfahrboerseDbContext _context;
    private readonly IHubContext<NotificationHub> _hubContext;

    public ProfilController(MitfahrboerseDbContext context, ILogger<ProfilController> logger, IAccessToken accessToken,
        IHubContext<NotificationHub> hubContext)
        : base(logger, accessToken, context)
    {
        _context = context;
        _hubContext = hubContext;
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

    [HttpPost]
    public async Task<IActionResult> CancelRide(int rideId)
    {
        var ride = await _context.t_Rides
            .Include(r => r.PersonRides)
            .ThenInclude(pr => pr.Person)
            .Include(r => r.FK_StartsAt_Position)
            .Include(r => r.FK_EndsAt_Position)
            .Include(r => r.FK_Driver_Person)
            .FirstOrDefaultAsync(r => r.RideId == rideId && r.FK_Driver_PersonId == personId);

        if (ride == null)
        {
            return Json(new { success = false, message = "Fahrt nicht gefunden..." });
        }

        var recipients = ride.PersonRides
            .Where(pr => pr.Status != 2 && pr.Status != 3)
            .ToList();

        if (ride.PersonRides != null && ride.PersonRides.Any())
        {
            _context.t_PersonRides.RemoveRange(ride.PersonRides);
        }
        _context.t_Rides.Remove(ride);
        await _context.SaveChangesAsync();

        foreach (var personRide in recipients)
        {
            var message = $"Die Fahrt von {ride.FK_StartsAt_Position.Description} nach {ride.FK_EndsAt_Position.Description} am {ride.RideDateTime.Date} um {ride.RideDateTime.TimeOfDay} wurde von {ride.FK_Driver_Person.FirstName} {ride.FK_Driver_Person.LastName} storniert.";
            await _hubContext.Clients.User(personRide.FK_PersonId)
                .SendAsync("ReceiveNotification", "Fahrt storniert!", message);
        }

        return Json(new { success = true, message = "Fahrt wurde erfolgreich storniert." });
    }

    [HttpPost]
    public async Task<IActionResult> LeaveRide(int rideId)
    {
        var personRide = await _context.t_PersonRides
            .FirstOrDefaultAsync(pr => pr.FK_RideId == rideId && pr.FK_PersonId == personId);

        if (personRide == null)
        {
            return Json(new { success = false, message = "Teilnahme nicht gefunden." });
        }

        _context.t_PersonRides.Remove(personRide);

        await _context.SaveChangesAsync();

        return Json(new { success = true, message = "Du hast deine Teilnahme erfolgreich abgesagt." });
    }

    public IActionResult Logout()
    {
        return SignOut(
            new AuthenticationProperties { RedirectUri = Url.Action("Index", "Profil") },
            OpenIdConnectDefaults.AuthenticationScheme,
            CookieAuthenticationDefaults.AuthenticationScheme);
    }
}