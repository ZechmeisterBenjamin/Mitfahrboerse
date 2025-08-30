using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        var rides = _context.t_Rides
            .Include(r => r.FK_Driver_Person)
            .Include(r => r.FK_StartsAt_Position)
            .Include(r => r.FK_EndsAt_Position)
            .ToList();

        return View(rides);
    }

    public IActionResult Create()
    {
        return View();
    }
}