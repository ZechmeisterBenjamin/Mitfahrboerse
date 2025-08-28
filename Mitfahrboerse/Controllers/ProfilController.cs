using Microsoft.AspNetCore.Mvc;

namespace Mitfahrboerse.Controllers;

public class ProfilController : Controller
{
    public IActionResult Index()
    {
        return View();   
    }
}