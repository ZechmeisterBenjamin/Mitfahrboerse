using Microsoft.AspNetCore.Mvc;

namespace Mitfahrboerse.Controllers;

public class ShopController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}