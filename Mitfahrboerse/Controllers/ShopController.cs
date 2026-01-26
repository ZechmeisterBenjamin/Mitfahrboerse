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

        string rndcode = RandomCodeGenerator();

        if (user != null && offer != null && user.Points >= price)
        {
            user.Points -= price;

            var person_offer = new t_PersonOffer
            {
                FK_OfferId = offer.OfferId,
                FK_PersonId = user.PersonId,
                FK_ValidUntil = offer.ValidUntil
            };
            _context.t_PersonOffers.Add(person_offer);
            _context.SaveChanges();
            return Json(new { success = true, message = $"Erfolgreich gekauft: {title}\nCode zum Einlösen: {rndcode}" });
        }

        return Json(new { success = false, message = $"Fehler: Nicht genügend Punkte!" });
    }

    public string RandomCodeGenerator()
    {
        string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        string numbers = "0123456789";
        bool exists;
        string randomstr = "";

        StringBuilder str = new StringBuilder();
        do
        {
            exists = false;
            str.Clear();

            for(int i = 0; i < 4; i++)
            {
                str.Append(chars[RandomNumberGenerator.GetInt32(0, chars.Length)]);
            }

            for(int i = 0; i < 4; i++)
            {
                str.Append(numbers[RandomNumberGenerator.GetInt32(0, numbers.Length)]);
            }

            randomstr = str.ToString();

            if (codes.Contains(randomstr))
            {
                exists = true;
            }
        } while (exists);

        codes.Add(randomstr);
        return randomstr;
    }
}