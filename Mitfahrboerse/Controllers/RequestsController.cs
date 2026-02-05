using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Mitfahrboerse.Interfaces;
using Mitfahrboerse.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Mitfahrboerse.Hubs;
namespace Mitfahrboerse.Controllers
{
    public class RequestsController : BaseController
    {
        private readonly MitfahrboerseDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;
        public RequestsController(
            MitfahrboerseDbContext context, 
            ILogger<RideController> logger, 
            IAccessToken accessToken,
            IHubContext<NotificationHub> hubContext)
            : base(logger, accessToken, context)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public async Task<IActionResult> Index()
        {
            var anfragen = await _context.t_PersonRides
                .Include(pr => pr.Person) 
                .Include(pr => pr.Ride)   
                    .ThenInclude(r => r.FK_StartsAt_Position)
                .Include(pr => pr.Ride)
                    .ThenInclude(r => r.FK_EndsAt_Position)
                .Where(pr => pr.Ride.FK_Driver_PersonId == personId && pr.Status == 0)
                .ToListAsync();

            return View(anfragen);
        }

        [HttpPost]
        public async Task<IActionResult> HandleAction(int rideId, string requesterId, short newStatus)
        {
            var anfrage = await _context.t_PersonRides
                .Include(pr => pr.Ride)
                .ThenInclude(r => r.FK_StartsAt_Position)
                .Include(pr => pr.Ride)
                .ThenInclude(r => r.FK_EndsAt_Position)
                .FirstOrDefaultAsync(pr => pr.FK_RideId == rideId && pr.FK_PersonId == requesterId);

            if (anfrage != null)
            {
                anfrage.Status = newStatus;
                await _context.SaveChangesAsync();
            }
            
            var message = $"Deine Anfrage für die Fahrt von {anfrage.Ride.FK_StartsAt_Position.Description} nach {anfrage.Ride.FK_EndsAt_Position.Description} am {anfrage.Ride.RideDateTime.Date} um {anfrage.Ride.RideDateTime.TimeOfDay} wurde {(newStatus == 2 ? "abgelehnt" : "akzeptiert")}";
            await _hubContext.Clients.User(requesterId).SendAsync("ReceiveNotification", $"Anfrage {(newStatus == 2 ? "abgelehnt" : "akzeptiert")}", message);
            return RedirectToAction(nameof(Index));
        }
    }
}
