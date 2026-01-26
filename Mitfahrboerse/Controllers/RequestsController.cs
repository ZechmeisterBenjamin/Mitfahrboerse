using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Mitfahrboerse.Interfaces;
using Mitfahrboerse.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mitfahrboerse.Controllers
{
    public class RequestsController : BaseController
    {
        private readonly MitfahrboerseDbContext _context;

        public RequestsController(MitfahrboerseDbContext context, ILogger<RideController> logger, IAccessToken accessToken) : base(logger, accessToken, context)
        {
            _context = context;
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
                .FirstOrDefaultAsync(pr => pr.FK_RideId == rideId && pr.FK_PersonId == requesterId);

            if (anfrage != null)
            {
                anfrage.Status = newStatus;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
