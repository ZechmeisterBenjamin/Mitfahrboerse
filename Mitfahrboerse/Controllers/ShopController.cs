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
    public async Task<IActionResult> EditOffer(int id, string newTitle, short newPrice, DateTime newValidUntil)
    {
        var offer = await _context.t_Offers.FirstOrDefaultAsync(o => o.OfferId == id);

        if (offer == null) return RedirectToAction("Index");

        bool alreadyPurchased = await _context.t_PersonOffers.AnyAsync(po => po.FK_OfferId == id);

        if (alreadyPurchased)
        {
            TempData["Error"] = "Ändern nicht möglich: Dieser Gutschein wurde bereits gekauft!";
            return RedirectToAction("Index");
        }

        offer.Title = newTitle;
        offer.Price = newPrice;
        offer.ValidUntil = DateOnly.FromDateTime(newValidUntil);

        _context.Update(offer);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Angebot erfolgreich aktualisiert.";
        return RedirectToAction("Index");
    }

    [HttpGet]
    [HttpGet]
    public async Task<string> GetVoucherPurchases(int id)
    {
        var purchases = await _context.t_PersonOffers
            .Include(po => po.FK_Person)
            .Where(po => po.FK_OfferId == id)
            .ToListAsync();

        if (!purchases.Any())
        {
            return "<p style='padding:10px;'>Diesen Gutschein hat noch niemand gekauft.</p>";
        }

        var html = "<table style='width: 100%; border-collapse: collapse;'>";
        foreach (var p in purchases)
        {
            html += $@"<tr style='border-bottom: 1px solid #444;'>
                    <td style='padding: 10px 5px; color: white;'>{p.FK_Person.FirstName} {p.FK_Person.LastName}</td>
                    <td style='padding: 10px 5px; font-family: monospace; color: #ffcc00;'>{p.Code}</td>
                  </tr>";
        }
        html += "</table>";

        return html; // Gibt fertiges HTML zurück
    }

    public string RandomCodeGenerator(string name, t_Offer offer)
    {
        string offername = offer.Title.Replace(" ", "");

        string rnd = new Random().Next(1000, 9999).ToString();

        return $"{name}-{offername}-{rnd}";
    }
}