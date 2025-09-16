using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mitfahrboerse.Models;

namespace Mitfahrboerse.Controllers;

public class ShopController : Controller
{
    private readonly MitfahrboerseDbContext _context;
    public ShopController(MitfahrboerseDbContext context)
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
}