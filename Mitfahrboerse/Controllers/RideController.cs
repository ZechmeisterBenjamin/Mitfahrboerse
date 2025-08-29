using Microsoft.AspNetCore.Mvc;
using Mitfahrboerse.Models;

namespace Mitfahrboerse.Controllers;

public class RideController : Controller
{
    private readonly MitfahrboerseDbContext _context;
    public RideController(MitfahrboerseDbContext context)
    {
        _context = context;
    }
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Create()
    {
        return View();
    }
}