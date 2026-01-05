using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mitfahrboerse.Interfaces;
using Mitfahrboerse.Models;

namespace Mitfahrboerse.Controllers;

public class ShopController : BaseController
{
    private readonly MitfahrboerseDbContext _context;
    public ShopController(MitfahrboerseDbContext context, ILogger<ShopController> logger, IAccessToken accessToken) : base(logger, accessToken, context) 
    {
        _context = context;
    }
    public IActionResult Index()
    { 
        var offers = _context.t_Offers
            .OrderBy(o => o.ValidUntil)
            .ToList();
        return View(offers);
    }

    [HttpPost]
    public IActionResult BuyVoucher(string title, int price)
    {
        var user = _context.t_People.FirstOrDefault(p => p.PersonId == personId);

        if (user != null && user.Points >= price)
        {
            user.Points -= price;
            _context.SaveChanges();
            return Json(new { success = true, message = $"Erfolgreich gekauft: {title}" });
        }

        return Json(new { success = false, message = "Fehler: Nicht genügend Punkte!" });
    }
}