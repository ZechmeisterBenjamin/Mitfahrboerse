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
                .Include(p => p.PersonRides.Where(pr => pr.Status == 0 || pr.Status == 1 && pr.Ride.RideDateTime >= DateTime.Now)).ThenInclude(pr => pr.Ride).ThenInclude(r => r.FK_StartsAt_Position)
                .Include(p => p.PersonRides.Where(pr => pr.Status == 0 || pr.Status == 1 && pr.Ride.RideDateTime >= DateTime.Now)).ThenInclude(pr => pr.Ride).ThenInclude(r => r.FK_EndsAt_Position)
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
                .FirstOrDefaultAsync(r => r.RideId == rideId && r.FK_Driver_PersonId == personId);

            if (ride != null)
            {
                if (ride.PersonRides != null && ride.PersonRides.Any())
                {
                    _context.t_PersonRides.RemoveRange(ride.PersonRides);
                }
                _context.t_Rides.Remove(ride);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Fahrt wurde erfolgreich storniert.";

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
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> LeaveRide(int rideId)
        {
            var personRide = await _context.t_PersonRides
                .FirstOrDefaultAsync(pr => pr.FK_RideId == rideId && pr.FK_PersonId == personId);

            if (personRide != null)
            {
                _context.t_PersonRides.Remove(personRide);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Teilnahme erfolgreich abgesagt.";
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
            }

            return RedirectToAction(nameof(Index));

            

        }

        [HttpGet]
        public async Task<IActionResult> GetPassengers(int rideId)
        {
            var passengers = await _context.t_PersonRides
                .Where(pr => pr.FK_RideId == rideId && pr.Status == 0)
                .Include(pr => pr.Person)
                .Select(pr => new {
                    name = pr.Person.FirstName + " " + pr.Person.LastName,
                    klasse = pr.Person.Class, 
                })
                .ToListAsync();

            return Json(passengers);
        }
    }
}
