using Microsoft.AspNetCore.Mvc;
using Mitfahrboerse.Models;

namespace Mitfahrboerse.Controllers;

public class ProfilController : Controller
{
    private readonly MitfahrboerseDbContext _context;
    public ProfilController(MitfahrboerseDbContext context)
    {
        _context = context;
    }
    public IActionResult Index()
    {
        return View();   
    }
}