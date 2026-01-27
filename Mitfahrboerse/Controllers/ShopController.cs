using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mitfahrboerse.Interfaces;
using Mitfahrboerse.Models;
using System.Security.Cryptography;
using System.Text;

namespace Mitfahrboerse.Controllers;

public class ShopController : BaseController
{
    private readonly MitfahrboerseDbContext _context;
    private List<string> codes = new List<string>();
    public ShopController(MitfahrboerseDbContext context, ILogger<ShopController> logger, IAccessToken accessToken) : base(logger, accessToken, context) 
    {
        _context = context;
    }
    public IActionResult Index()
    { 
        var offers = _context.t_Offers
            .OrderBy(o => o.ValidUntil)
            .ToList();
        return View(offers);
    }

    [HttpPost]
    public IActionResult BuyVoucher(string title, int price)
    {
        var user = _context.t_People.FirstOrDefault(p => p.PersonId == personId);
        var offer = _context.t_Offers.FirstOrDefault(p => p.Title == title);
        string username = user.LastName;
        int offerId = offer.OfferId;

        string rndcode = RandomCodeGenerator(username, offerId);

        if (user != null && offer != null && user.Points >= price)
        {
            user.Points -= price;

            var person_offer = new t_PersonOffer
            {
                FK_OfferId = offer.OfferId,
                FK_PersonId = user.PersonId,
                FK_ValidUntil = offer.ValidUntil,
                Code = rndcode,
            };
            _context.t_PersonOffers.Add(person_offer);
            _context.SaveChanges();
            return Json(new { success = true, message = $"Erfolgreich gekauft: {title}\nCode zum Einlösen: {rndcode}" });
        }

        return Json(new { success = false, message = $"Fehler: Nicht genügend Punkte!" });
    }

    

    public string RandomCodeGenerator(string name, int offerId)
    {
        string randomcode = $"{name}_{offerId}";
        return randomcode;
    }
}