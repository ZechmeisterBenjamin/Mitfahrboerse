using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mitfahrboerse.Interfaces;
using Mitfahrboerse.Models;
using System;
using System.Threading.Tasks;

namespace Mitfahrboerse.Controllers;

public class RideController : BaseController
{
    private readonly MitfahrboerseDbContext _context;
    public RideController(MitfahrboerseDbContext context, ILogger<RideController> logger, IAccessToken accessToken) : base(logger, accessToken)
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

        var selectedRide = selectedRideId.HasValue 
            ? rides.FirstOrDefault(r => r.RideId == selectedRideId.Value)
            : rides.FirstOrDefault();

        ViewBag.SelectedRide = selectedRide;
        
        return View(rides);
    }

    public IActionResult Create()
    {
        ViewBag.Positions = _context.t_Positions.ToList();
        return View();
    }
    
    [HttpPost]
    public async Task<IActionResult> Create(t_Ride ride, int startPositionId, int endPositionId)
    {
        if (ModelState.IsValid)
        {
            try
            {
                int driverId = 1; 
                
                ride.FK_Driver_PersonId = driverId.ToString();
                
                ride.FK_StartsAt_PositionId = startPositionId;
                ride.FK_EndsAt_PositionId = endPositionId;
                
                _context.t_Rides.Add(ride);
                _context.SaveChanges();

                TempData["Message"] = "Fahrt erfolgreich erstellt!";
                return RedirectToAction(nameof(Index));

                
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Es ist ein Fehler aufgetreten: " + ex.Message);
            }
        }
        
        ViewBag.Positions = _context.t_Positions.ToList();
        return View(ride);
    }
}