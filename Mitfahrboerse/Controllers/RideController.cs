using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mitfahrboerse.Interfaces;
using Mitfahrboerse.Models;
using Mitfahrboerse.Services;
using System;
using System.Globalization;
using Microsoft.AspNetCore.SignalR;
using Mitfahrboerse.Hubs;

namespace Mitfahrboerse.Controllers;

public class RideController : BaseController
{
    private readonly MitfahrboerseDbContext _context;
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly IRouteMatchService _routeMatchService;
    
    public RideController(MitfahrboerseDbContext context, ILogger<RideController> logger, IAccessToken accessToken, IHubContext<NotificationHub> hubContext, IRouteMatchService routeMatchService) : base(logger, accessToken, context)
    {
        _context = context;
        _hubContext = hubContext;
        _routeMatchService = routeMatchService;
    }
            
            
    public IActionResult Index(int? selectedRideId = null, string? startLat = null, string? startLon = null, string? endLat = null, string? endLon = null)
    {
        DateTime now = DateTime.Now;

        var rides = _context.t_Rides
            //.Where(r => r.RideDateTime >= now || r.RideDateTime == now)
            //.Where(r => r.FK_Driver_PersonId != personId)
            .Include(r => r.FK_Driver_Person)
            .Include(r => r.FK_StartsAt_Position)
            .Include(r => r.FK_EndsAt_Position)
            .Include(r => r.FK_Car)
            .Include(r => r.PersonRides)
            .ThenInclude(pr => pr.Person)
            .ToList();

        // Intelligente Fahrtsuche: Wenn Koordinaten vorhanden, finde Fahrten mit Umweg-Berechnung
        List<RideWithDetourInfo> matchingRides = new();
        
        if (!string.IsNullOrEmpty(startLat) && !string.IsNullOrEmpty(startLon) && 
            !string.IsNullOrEmpty(endLat) && !string.IsNullOrEmpty(endLon))
        {
            // Parse die Koordinaten
            if (decimal.TryParse(startLat.Replace(",", "."), System.Globalization.CultureInfo.InvariantCulture, out decimal passengerStartLat) &&
                decimal.TryParse(startLon.Replace(",", "."), System.Globalization.CultureInfo.InvariantCulture, out decimal passengerStartLon) &&
                decimal.TryParse(endLat.Replace(",", "."), System.Globalization.CultureInfo.InvariantCulture, out decimal passengerEndLat) &&
                decimal.TryParse(endLon.Replace(",", "."), System.Globalization.CultureInfo.InvariantCulture, out decimal passengerEndLon))
            {
                // Nutze den Service, um passende Fahrten zu finden
                matchingRides = _routeMatchService.FindMatchingRides(
                    rides,
                    passengerStartLat,
                    passengerStartLon,
                    passengerEndLat,
                    passengerEndLon
                );
                
                // Speichere die Umweg-Informationen in ViewBag für die View
                ViewBag.MatchingRides = matchingRides;
                ViewBag.HasSearch = true;
                rides = matchingRides.Select(m => m.Ride).OrderBy(r => r.RideDateTime).ToList();
            }
        }
        else
        {
            // Keine Suche aktiv - zeige alle Fahrten sortiert nach Zeit
            rides = rides.OrderBy(r => r.RideDateTime).ToList();
            ViewBag.HasSearch = false;
        }

        var selectedRide = selectedRideId.HasValue 
            ? rides.FirstOrDefault(r => r.RideId == selectedRideId.Value)
            : rides.FirstOrDefault();

        ViewBag.SelectedRide = selectedRide;
        ViewBag.MatchingRidesDict = matchingRides.ToDictionary(m => m.Ride.RideId, m => m);
            
        return View(rides);
    }

    public IActionResult Create()
    {
        var userCars = _context.t_Cars.Where(c => c.FK_Owner_PersonId == personId).ToList();
                
        ViewBag.UserCars = userCars;
        ViewBag.Positions = _context.t_Positions.ToList();
        return View();
    }
            
    [HttpPost]
    public async Task<IActionResult> Create(
        string startPositionDescription, string startLat, string startLon,
        string endPositionDescription, string endLat, string endLon,
        DateTime rideDateTime, double routeLength, int carId)
    {
        try
        {
            if (string.IsNullOrEmpty(personId))
            {
                return Challenge(); 
            }

            int startPositionId = GetOrCreatePosition(startPositionDescription, decimal.Parse(startLat.Replace(",", "."), CultureInfo.InvariantCulture), decimal.Parse(startLon.Replace(",", "."), CultureInfo.InvariantCulture));
            int endPositionId = GetOrCreatePosition(endPositionDescription, decimal.Parse(endLat.Replace(",", "."), CultureInfo.InvariantCulture), decimal.Parse(endLon.Replace(",", "."), CultureInfo.InvariantCulture));

        var ride = new t_Ride
            {
                FK_Driver_PersonId = personId,
                FK_StartsAt_PositionId = startPositionId, 
                FK_EndsAt_PositionId = endPositionId,    
                RideDateTime = rideDateTime,
                Distance = (int)(routeLength * 1000), 
                Status = 0,
                FK_CarId = carId
            };

            _context.t_Rides.Add(ride);
            _context.SaveChanges();
                    
                    
            TempData["Message"] = "Fahrt erfolgreich erstellt!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Erstellen der Fahrt.");
            ModelState.AddModelError("", "Es ist ein Fehler aufgetreten: " + ex.Message);
        }
            
        ViewBag.UserCars = _context.t_Cars.Where(c => c.FK_Owner_PersonId == personId).ToList();
        ViewBag.Positions = _context.t_Positions.ToList();
        return View();
    }
    private int GetOrCreatePosition(string description, decimal latitude, decimal longitude)
    {
        var position = _context.t_Positions
            .FirstOrDefault(p => p.Latitude == latitude && p.Longitude == longitude);

        if (position != null)
        {
            return position.PositionId;
        }
        int nextId = 1;
        if (_context.t_Positions.Any())
        {
            nextId = _context.t_Positions.Max(p => p.PositionId) + 1;
        }

        var newPosition = new t_Position
        {
            PositionId = nextId,
            Description = description,
            Latitude = latitude,
            Longitude = longitude
        };

        _context.t_Positions.Add(newPosition);
        _context.SaveChanges(); 

        return newPosition.PositionId;
    }
            
            
    [HttpPost]
    public async Task<IActionResult> RequestRide(int rideId)
    {
        try
        {
            if (string.IsNullOrEmpty(personId)) return Challenge();

            var ride = await _context.t_Rides
                .Include(r => r.FK_Driver_Person)
                .Include(r => r.FK_StartsAt_Position)
                .Include(r => r.FK_EndsAt_Position)   
                .FirstOrDefaultAsync(r => r.RideId == rideId);

            if (ride == null)
            {
                return Json(new { success = false, message = "Fahrt nicht gefunden." });
            }

            // TODO: Bei realem Betrieb auskommentieren
            // if (ride.FK_Driver_PersonId == personId)
            // {
            //     return Json(new { success = false, message = "Du kannst nicht bei deiner eigenen Fahrt mitfahren." });
            // }

            

            bool alreadyRequested = await _context.t_PersonRides
                .AnyAsync(pr => pr.FK_RideId == rideId && pr.FK_PersonId == personId);

            if (alreadyRequested)
            {
                return Json(new { success = false, message = "Du hast diese Fahrt bereits angefragt." });
            }

            _context.t_PersonRides.Add(new t_PersonRide(personId, rideId, 0));
            await _context.SaveChangesAsync();

            if (!string.IsNullOrEmpty(ride.FK_Driver_PersonId))
            {
                var from = ride.FK_StartsAt_Position?.Description ?? "Start";
                var to = ride.FK_EndsAt_Position?.Description ?? "Ziel";
                var time = ride.RideDateTime.ToString("HH:mm");
                var date = ride.RideDateTime.ToString("dd.MM.");

                await _hubContext.Clients.User(ride.FK_Driver_PersonId).SendAsync(
                    "ReceiveNotification", 
                    "Neue Mitfahranfrage", 
                    $"Jemand möchte bei deiner Fahrt von {from} nach {to} am {date} um {time} mitfahren!"
                );
            }
        
            return Json(new { success = true, message = "Anfrage erfolgreich gesendet!" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler in RequestRide für Fahrt {RideId}", rideId);
            return Json(new { success = false, message = "Ein technischer Fehler ist aufgetreten." });
        }
    }
}