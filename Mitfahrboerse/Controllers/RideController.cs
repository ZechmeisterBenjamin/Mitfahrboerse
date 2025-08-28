using Microsoft.AspNetCore.Mvc;

namespace Mitfahrboerse.Controllers;

public class RideController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Create()
    {
        return View();
    }
}