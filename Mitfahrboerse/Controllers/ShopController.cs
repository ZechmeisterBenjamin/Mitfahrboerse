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
        DateOnly now = DateOnly.FromDateTime(DateTime.Now);

        var offers = _context.t_Offers
            .Where(o => o.ValidUntil >= now)
            .OrderBy(o => o.ValidUntil)
            .ToList();
        return View(offers);
    }

    [HttpPost]
    public IActionResult BuyVoucher(int offerId)
    {
        var user = _context.t_People.FirstOrDefault(p => p.PersonId == personId);
        var offer = _context.t_Offers.FirstOrDefault(p => p.OfferId == offerId);

        if (offer == null || user == null) return RedirectToAction("Index");

        bool alreadyOwned = _context.t_PersonOffers.Any(po => po.FK_OfferId == offerId && po.FK_PersonId == personId);
        if (alreadyOwned)
        {
            TempData["Error"] = "Du besitzt diesen Gutschein bereits!";
            return RedirectToAction("Index");
        }

        // Prüfung: Punkte
        if (user.Points >= offer.Price)
        {
            user.Points -= (int)offer.Price;

            var person_offer = new t_PersonOffer
            {
                FK_OfferId = offer.OfferId,
                FK_PersonId = user.PersonId,
                FK_ValidUntil = offer.ValidUntil,
                Code = RandomCodeGenerator(user.LastName, offer),
            };

            _context.t_PersonOffers.Add(person_offer);
            _context.SaveChanges();

            TempData["Success"] = $"Gutschein '{offer.Title}' erfolgreich gekauft!";
        }
        else
        {
            TempData["Error"] = "Nicht genügend Punkte!";
        }

        return RedirectToAction("Index");
    }

    [HttpPost]
    public IActionResult DeleteOffer(int id)
    {
        var alreadyPurchased = _context.t_PersonOffers.Any(po => po.FK_OfferId == id);
        if (alreadyPurchased)
        {
            TempData["Error"] = "Löschen nicht möglich: Gutschein wurde bereits gekauft!";
            return RedirectToAction("Index");
        }

        var offer = _context.t_Offers.FirstOrDefault(o => o.OfferId == id);
        if (offer != null)
        {
            _context.t_Offers.Remove(offer);
            _context.SaveChanges();
            TempData["Success"] = "Angebot wurde gelöscht.";
        }

        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> EditOffer(int id, DateTime oldValidUntil, string newTitle, short newPrice, DateTime newValidUntil)
    {
        var oldDate = DateOnly.FromDateTime(oldValidUntil);
        var offer = await _context.t_Offers
            .FirstOrDefaultAsync(o => o.OfferId == id);

        bool alreadyPurchased = await _context.t_PersonOffers
                .AnyAsync(po => po.FK_OfferId == id);

        if (alreadyPurchased)
        {
            return Json(new
            {
                success = false,
                message = "Ändern nicht möglich: Dieser Gutschein wurde bereits gekauft!"
            });
        }

        if (offer != null)
        {
            offer.Title = newTitle;
            offer.Price = newPrice;
            offer.ValidUntil = DateOnly.FromDateTime(newValidUntil);

            _context.Update(offer);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Angebot aktualisiert." });
        }
        return Json(new { success = false, message = "Fehler beim Aktualisieren." });
    }

    [HttpGet]
    public async Task<IActionResult> GetVoucherPurchases(int id)
    {
        var purchases = await _context.t_PersonOffers
            .Include(po => po.FK_Person)
            .Where(po => po.FK_OfferId == id)
            .Select(po => new {
                UserName = po.FK_Person.FirstName + " " + po.FK_Person.LastName,
                Code = po.Code
            })
            .ToListAsync();

        return Json(purchases);
    }

    public string RandomCodeGenerator(string name, t_Offer offer)
    {
        string offername = offer.Title.Replace(" ", "");

        string rnd = new Random().Next(1000, 9999).ToString();

        return $"{name}-{offername}-{rnd}";
    }
}