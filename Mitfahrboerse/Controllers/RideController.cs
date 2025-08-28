using Microsoft.AspNetCore.Mvc;

namespace Mitfahrboerse.Controllers;

public class RideController : Controller
{
    public IActionResult Index()
    {
        return View("Mitfahren");
    }

    public IActionResult Create()
    {
        return View("Fahrt_erstellen");
    }
}