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
public IActionResult Index(int? selectedRideId = null)
{
    var rides = _context.t_Rides
        .Include(r => r.FK_Driver_Person)
        .Include(r => r.FK_StartsAt_Position)
        .Include(r => r.FK_EndsAt_Position)
        .Include(r => r.FK_People)
        .ToList();

    // Get the selected ride or default to the first one
    var selectedRide = selectedRideId.HasValue 
        ? rides.FirstOrDefault(r => r.RideId == selectedRideId.Value)
        : rides.FirstOrDefault();

    ViewBag.SelectedRide = selectedRide;
    
    return View(rides);
}

    public IActionResult Create()
    {
        return View();
    }
}