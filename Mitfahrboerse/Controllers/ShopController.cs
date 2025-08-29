using Microsoft.AspNetCore.Mvc;
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
        var offers = _context.t_Offers.ToList();
        return View();
    }
}