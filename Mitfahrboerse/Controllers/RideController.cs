    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Mitfahrboerse.Interfaces;
    using Mitfahrboerse.Models;
    using System;

    namespace Mitfahrboerse.Controllers;

    public class RideController : BaseController
    {
        private readonly MitfahrboerseDbContext _context;
        public RideController(MitfahrboerseDbContext context, ILogger<RideController> logger, IAccessToken accessToken) : base(logger, accessToken, context)
        {
            _context = context;
        }
        
        public IActionResult Index(int? selectedRideId = null)
        {
            var rides = _context.t_Rides
                .Include(r => r.FK_Driver_Person)
                .Include(r => r.FK_StartsAt_Position)
                .Include(r => r.FK_EndsAt_Position)
                .Include(r => r.PersonRides)
                .ThenInclude(pr => pr.Person)
                .ToList();

            var selectedRide = selectedRideId.HasValue 
                ? rides.FirstOrDefault(r => r.RideId == selectedRideId.Value)
                : rides.FirstOrDefault();

            ViewBag.SelectedRide = selectedRide;
            
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
        public IActionResult Create(
            string startPositionDescription, decimal startLat, decimal startLon,
            string endPositionDescription, decimal endLat, decimal endLon,
            DateTime rideDateTime, double routeLength, int carId)
        {
            try
            {
                if (string.IsNullOrEmpty(personId))
                {
                    return Challenge(); 
                }

                int startPositionId = GetOrCreatePosition(startPositionDescription, startLat, startLon);
                int endPositionId = GetOrCreatePosition(endPositionDescription, endLat, endLon);
            
                var ride = new t_Ride
                {
                    FK_Driver_PersonId = personId,
                    FK_StartsAt_PositionId = startPositionId, 
                    FK_EndsAt_PositionId = endPositionId,    
                    RideDateTime = rideDateTime,
                    Distance = (int)(routeLength * 1000), 
                    Status = 0
                    // FK_CarId = carId
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
        public IActionResult RequestRide(int rideId)
        {
            try
            {
                var ride = _context.t_Rides
                    .Include(r => r.FK_Driver_Person)
                    .FirstOrDefault(r => r.RideId == rideId);

                if (ride == null)
                {
                    return Json(new { success = false, message = $"Fahrt ({rideId}) nicht gefunden." });
                }

                _context.t_PersonRides.Add(new t_PersonRide(personId, rideId, 0));
                _context.SaveChanges();
                
                return Json(new { success = true, message = "Anfrage erfolgreich gesendet!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error requesting ride {RideId}", rideId);
                return Json(new { success = false, message = "Es ist ein Fehler aufgetreten." });
            }
        }
    }