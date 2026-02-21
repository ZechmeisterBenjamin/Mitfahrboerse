using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mitfahrboerse.Interfaces;
using Mitfahrboerse.Models;
using Mitfahrboerse.Services;
using System;
using System.Globalization;
using System.Text.Json;
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
            
            
    [HttpGet]
    public async Task<IActionResult> Geocode(string address)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(address))
            {
                return Json(new { success = false, message = "Adresse fehlt." });
            }

            var url = $"https://nominatim.openstreetmap.org/search?format=json&q={Uri.EscapeDataString(address)}&countrycodes=AT";
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "Mitfahrboerse Ride-Sharing App");

            var response = await httpClient.GetStringAsync(url);
            using var doc = JsonDocument.Parse(response);
            var results = doc.RootElement;

            if (results.GetArrayLength() > 0)
            {
                var first = results[0];
                var lat = first.GetProperty("lat").GetString();
                var lon = first.GetProperty("lon").GetString();
                return Json(new { success = true, lat, lon });
            }

            return Json(new { success = false, message = "Adresse nicht gefunden." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Geocoding-Fehler für Adresse: {Address}", address);
            return Json(new { success = false, message = "Geocoding-Fehler." });
        }
    }

    public async Task<IActionResult> Index(int? selectedRideId = null, string? startLat = null, string? startLon = null, string? endLat = null, string? endLon = null)
    {
        DateTime now = DateTime.Now;

        var rides = _context.t_Rides
            .Include(r => r.FK_Driver_Person)
            .Include(r => r.FK_StartsAt_Position)
            .Include(r => r.FK_EndsAt_Position)
            .Include(r => r.FK_Car)
            .Include(r => r.PersonRides)
            .ThenInclude(pr => pr.Person)
            .ToList();

        List<RideWithDetourInfo> matchingRides = new();
        
        if (!string.IsNullOrEmpty(startLat) && !string.IsNullOrEmpty(startLon) && 
            !string.IsNullOrEmpty(endLat) && !string.IsNullOrEmpty(endLon))
        {
            if (decimal.TryParse(startLat.Replace(",", "."), CultureInfo.InvariantCulture, out decimal passengerStartLat) &&
                decimal.TryParse(startLon.Replace(",", "."), CultureInfo.InvariantCulture, out decimal passengerStartLon) &&
                decimal.TryParse(endLat.Replace(",", "."), CultureInfo.InvariantCulture, out decimal passengerEndLat) &&
                decimal.TryParse(endLon.Replace(",", "."), CultureInfo.InvariantCulture, out decimal passengerEndLon))
            {
                matchingRides = _routeMatchService.FindMatchingRides(
                    rides,
                    passengerStartLat,
                    passengerStartLon,
                    passengerEndLat,
                    passengerEndLon
                );
                
                ViewBag.MatchingRides = matchingRides;
                ViewBag.HasSearch = true;
                rides = matchingRides.Select(m => m.Ride).OrderBy(r => r.RideDateTime).ToList();
            }
        }
        else
        {
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

    public IActionResult Create(string? startPos = null, string? endPos = null, string? dateTime = null)
    {
        var userCars = _context.t_Cars.Where(c => c.FK_Owner_PersonId == personId).ToList();

        ViewBag.UserCars = userCars;
        ViewBag.Positions = _context.t_Positions.ToList();

        ViewBag.SavedStartPos = startPos ?? "";
        ViewBag.SavedEndPos = endPos ?? "";
        ViewBag.SavedDateTime = dateTime ?? "";

        return View();
    }

    [HttpPost]
    public IActionResult CreateCarAndReturn(string kennzeichen, short sitze, string marke, string modell, string farbe,
        string? startPos = null, string? endPos = null, string? dateTime = null)
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

        TempData["Message"] = "Auto erfolgreich erstellt!";
        TempData["NewCarId"] = newCar.CarId;

        return RedirectToAction("Create", new { startPos, endPos, dateTime });
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

            if (rideDateTime < DateTime.Now)
            {
                ModelState.AddModelError("", "Das Fahrtdatum kann nicht in der Vergangenheit liegen.");
                ViewBag.UserCars = _context.t_Cars.Where(c => c.FK_Owner_PersonId == personId).ToList();
                ViewBag.Positions = _context.t_Positions.ToList();
                return View();
            }

            int startPositionId = await GetOrCreatePositionAsync(startPositionDescription, decimal.Parse(startLat.Replace(",", "."), CultureInfo.InvariantCulture), decimal.Parse(startLon.Replace(",", "."), CultureInfo.InvariantCulture));
            int endPositionId = await GetOrCreatePositionAsync(endPositionDescription, decimal.Parse(endLat.Replace(",", "."), CultureInfo.InvariantCulture), decimal.Parse(endLon.Replace(",", "."), CultureInfo.InvariantCulture));

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
            await _context.SaveChangesAsync();

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
    
    private async Task<string> ReverseGeocodeAsync(decimal latitude, decimal longitude)
    {
        var latStr = latitude.ToString(CultureInfo.InvariantCulture);
        var lonStr = longitude.ToString(CultureInfo.InvariantCulture);
        var url = $"https://nominatim.openstreetmap.org/reverse?format=json&lat={latStr}&lon={lonStr}";

        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("User-Agent", "Mitfahrboerse Ride-Sharing App");

        try
        {
            var response = await httpClient.GetStringAsync(url);
            using (JsonDocument doc = JsonDocument.Parse(response))
            {
                JsonElement root = doc.RootElement;
                if (root.TryGetProperty("display_name", out JsonElement displayNameElement))
                {
                    return displayNameElement.GetString() ?? "Unbekannter Ort";
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Reverse geocoding failed for lat={lat}, lon={lon}", latitude, longitude);
        }

        return "Unbekannter Ort";
    }

    private async Task<int> GetOrCreatePositionAsync(string? description, decimal latitude, decimal longitude)
    {
        var position = await _context.t_Positions
            .FirstOrDefaultAsync(p => p.Latitude == latitude && p.Longitude == longitude);

        if (position != null)
        {
            return position.PositionId;
        }

        if (string.IsNullOrEmpty(description))
        {
            description = await ReverseGeocodeAsync(latitude, longitude);
        }
    
        int nextId = 1;
        if (await _context.t_Positions.AnyAsync())
        {
            nextId = await _context.t_Positions.MaxAsync(p => p.PositionId) + 1;
        }

        var newPosition = new t_Position
        {
            PositionId = nextId,
            Description = description,
            Latitude = latitude,
            Longitude = longitude
        };

        _context.t_Positions.Add(newPosition);
        await _context.SaveChangesAsync(); 

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

            _context.t_PersonRides.Add(new t_PersonRide(personId, rideId, 1));
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