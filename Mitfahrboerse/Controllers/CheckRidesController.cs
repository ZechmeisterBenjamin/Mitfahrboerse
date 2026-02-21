using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Mitfahrboerse.Hubs;
using Mitfahrboerse.Interfaces;
using Mitfahrboerse.Models;

namespace Mitfahrboerse.Controllers
{
    public class CheckRidesController : BaseController
    {
        private readonly MitfahrboerseDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;

        public CheckRidesController(MitfahrboerseDbContext context, ILogger<CheckRidesController> logger, IAccessToken accessToken, IHubContext<NotificationHub> hubContext)
            : base(logger, accessToken, context)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _context.t_People
                .Include(p => p.t_Rides).ThenInclude(r => r.FK_StartsAt_Position)
                .Include(p => p.t_Rides).ThenInclude(r => r.FK_EndsAt_Position)
                .Include(p => p.t_Rides).ThenInclude(r => r.PersonRides)
                .Include(p => p.PersonRides).ThenInclude(pr => pr.Ride).ThenInclude(r => r.FK_StartsAt_Position)
                .Include(p => p.PersonRides).ThenInclude(pr => pr.Ride).ThenInclude(r => r.FK_EndsAt_Position)
                .FirstOrDefaultAsync(p => p.PersonId == personId);

            if (user?.PersonRides != null)
            {
                var rideIds = user.PersonRides.Select(pr => pr.FK_RideId).ToList();
                await _context.t_Rides
                    .Include(r => r.FK_Driver_Person)
                    .Where(r => rideIds.Contains(r.RideId))
                    .ToListAsync();
            }

            return View(user);
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

            /*
            var calendarService = new CalendarEvent(_accessToken);
            if (!string.IsNullOrEmpty(ride.EventId))
            {
                await calendarService.DeleteEventAsync(ride.EventId);
            }

            foreach (var pr in ride.PersonRides)
            {
                if (!string.IsNullOrEmpty(pr.EventId))
                {
                    await calendarService.DeleteEventAsync(pr.EventId);
                }
            }
            */

            if (ride.PersonRides != null && ride.PersonRides.Any())
            {
                _context.t_PersonRides.RemoveRange(ride.PersonRides);
            }
            _context.t_Rides.Remove(ride);
            await _context.SaveChangesAsync();

            // Fahrt-Details für die Erfolgs-Meldung
            var rideDetails = $"{ride.RideDateTime.ToString("dd.MM.yyyy, HH:mm")} - {ride.FK_StartsAt_Position.Description} → {ride.FK_EndsAt_Position.Description}";

            return Json(new { success = true, message = "Fahrt wurde erfolgreich storniert.", rideDetails = rideDetails });
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

            /*
            var calendarService = new CalendarEvent(_accessToken);
            if (!string.IsNullOrEmpty(ride.EventId))
            {
                await calendarService.DeleteEventAsync(ride.EventId);
            }

            foreach (var pr in ride.PersonRides)
            {
                if (!string.IsNullOrEmpty(pr.EventId))
                {
                    await calendarService.DeleteEventAsync(pr.EventId);
                }
            } 
            */

            _context.t_PersonRides.Remove(personRide);

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Du hast deine Teilnahme erfolgreich abgesagt." });
        }

        [HttpGet]
        public async Task<IActionResult> GetPassengers(int rideId)
        {
            var passengers = await _context.t_PersonRides
                .Where(pr => pr.FK_RideId == rideId && pr.Status == 0)
                .Include(pr => pr.Person)
                .Select(pr => new {
                    name = pr.Person.FirstName + " " + pr.Person.LastName,
                    klasse = pr.Person.Class 
                })
                .ToListAsync();

            return Json(passengers);
        }
    }
}
