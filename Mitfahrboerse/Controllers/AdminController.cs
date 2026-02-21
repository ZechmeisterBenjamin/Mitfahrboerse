using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mitfahrboerse.Interfaces;
using Mitfahrboerse.Models;

namespace Mitfahrboerse.Controllers
{
    public class AdminController : BaseController
    {
        private readonly MitfahrboerseDbContext _context;

        public AdminController(ILogger<AdminController> logger, IAccessToken accessToken, MitfahrboerseDbContext context)
            : base(logger, accessToken, context)
        {
            _context = context;
        }

        // Private Hilfsmethode zur Admin-Authentifizierung
        private async Task<IActionResult> CheckAdminAuthorizationAsync()
        {
            if (string.IsNullOrEmpty(personId))
            {
                return Challenge();
            }

            var person = await _context.t_People.FirstOrDefaultAsync(p => p.PersonId == personId);
            if (person == null || !person.IsAdmin)
            {
                return Forbid();
            }

            return null;
        }

        public async Task<IActionResult> Index()
        {
            var authResult = await CheckAdminAuthorizationAsync();
            if (authResult != null) return authResult;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateVoucher(string description, short price, DateTime validUntil)
        {
            var authResult = await CheckAdminAuthorizationAsync();
            if (authResult != null) return authResult;

            try
            {
                int nextId = await _context.t_Offers.AnyAsync()
                     ? await _context.t_Offers.MaxAsync(o => o.OfferId) + 1
                     : 1;

                var newOffer = new t_Offer
                {
                    OfferId = nextId,
                    Title = description,
                    Price = price,
                    ValidUntil = DateOnly.FromDateTime(validUntil)
                };

                _context.t_Offers.Add(newOffer);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Gutschein erfolgreich erstellt!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Fehler: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetStatistics()
        {
            var authResult = await CheckAdminAuthorizationAsync();
            if (authResult != null) return authResult;

            try
            {
                var totalRides = await _context.t_Rides.CountAsync();
                var totalDistance = await _context.t_Rides.SumAsync(r => r.Distance);
                var totalPassengers = await _context.t_PersonRides.CountAsync(pr => pr.Status == 1);

                return Json(new
                {
                    success = true,
                    totalRides = totalRides,
                    totalDistance = totalDistance,
                    totalPassengers = totalPassengers
                });
            }
            catch
            {
                return Json(new { success = false });
            }
        }
    }
}
