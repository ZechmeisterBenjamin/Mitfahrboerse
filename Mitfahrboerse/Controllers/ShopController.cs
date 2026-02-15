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
    public IActionResult BuyVoucher(int offerId, int price)
    {
        var user = _context.t_People.FirstOrDefault(p => p.PersonId == personId);
        var offer = _context.t_Offers.FirstOrDefault(p => p.OfferId == offerId);

        string rndcode = RandomCodeGenerator(user.LastName, offer);

        if (user.Points >= offer.Price)
        {
            user.Points -= (int)offer.Price;

            var person_offer = new t_PersonOffer
            {
                FK_OfferId = offer.OfferId,
                FK_PersonId = user.PersonId,
                FK_ValidUntil = offer.ValidUntil,
                Code = rndcode,
            };

            _context.t_PersonOffers.Add(person_offer);
            _context.SaveChanges();

            return Json(new { success = true, message = $"Erfolgreich gekauft: {offer.Title}\nCode zum Einlösen: {rndcode}" });
        }

        return Json(new { success = false, message = "Fehler: Nicht genügend Punkte!" });
    }

    [HttpPost]
    public async Task<IActionResult> DeleteOffer(int id, string validUntil)
    {
        try
        {
            if (!DateOnly.TryParse(validUntil, out DateOnly dateOnly))
            {
                return Json(new { success = false, message = "Ungültiges Datumsformat." });
            }

            bool alreadyPurchased = await _context.t_PersonOffers
                .AnyAsync(po => po.FK_OfferId == id);

            if (alreadyPurchased)
            {
                return Json(new
                {
                    success = false,
                    message = "Löschen nicht möglich: Dieser Gutschein wurde bereits  gekauft!"
                });
            }

            var offer = await _context.t_Offers
                .FirstOrDefaultAsync(o => o.OfferId == id && o.ValidUntil == dateOnly);

            if (offer != null)
            {
                _context.t_Offers.Remove(offer);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Angebot wurde erfolgreich gelöscht." });
            }

            return Json(new { success = false, message = "Angebot nicht gefunden." });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "Fehler: " + ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> EditOffer(int id, DateTime oldValidUntil, string newTitle, short newPrice, DateTime newValidUntil)
    {
        var oldDate = DateOnly.FromDateTime(oldValidUntil);
        var offer = await _context.t_Offers
            .FirstOrDefaultAsync(o => o.OfferId == id && o.ValidUntil == oldDate);

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