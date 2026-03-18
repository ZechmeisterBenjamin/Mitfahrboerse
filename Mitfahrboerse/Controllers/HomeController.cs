using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Mitfahrboerse.Interfaces;
using Mitfahrboerse.Models;
using Newtonsoft.Json.Linq;
using RestSharp;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Runtime.InteropServices.JavaScript;
using System.Text.Json.Serialization;

namespace Mitfahrboerse.Controllers;

[AllowAnonymous] 
public class HomeController : BaseController
{
    private readonly MitfahrboerseDbContext _context;
    public HomeController(MitfahrboerseDbContext context, ILogger<HomeController> logger, IAccessToken accessToken) : base(logger, accessToken, context)
    {
        _context = context;
    }


    [AllowAnonymous] 
    public async Task<IActionResult> Index(string vouchercode)
    {
        if (!string.IsNullOrEmpty(vouchercode))
        {
            try
            {
                var voucher = await _context.t_PersonOffers
                    .Include(v => v.FK_Offer)
                    .FirstOrDefaultAsync(v => v.Code == vouchercode);

                if (voucher == null)
                {
                    ViewBag.VoucherStatus = "error";
                    ViewBag.VoucherMessage = "Code nicht gefunden.";
                }
                else if (voucher.IsUsed)
                {
                    ViewBag.VoucherStatus = "warning";
                    ViewBag.VoucherMessage = $"'{voucher.FK_Offer?.Title}' wurde bereits eingelöst.";
                }
                else
                {
                    voucher.IsUsed = true;
                    _context.SaveChanges();
                    ViewBag.VoucherStatus = "success";
                    ViewBag.VoucherMessage = $"Erfolg! '{voucher.FK_Offer?.Title}' aktiviert.";
                }
            }
            catch (Exception)
            {
                ViewBag.VoucherStatus = "error";
                ViewBag.VoucherMessage = "Datenbankfehler.";
            }

            return View();
        }

        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Start");
        }

        return View();
    }



    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
    [Authorize]
    public IActionResult Start()
    {
        var person = _context.t_People.FirstOrDefault(p => p.PersonId == personId);

        if (person == null) 
        {
            return RedirectToAction("Index", "Ride");
        }

        switch (person.Startpage)
        {
            case 1:
                return RedirectToAction("Create", "Ride");
            default:
                return RedirectToAction("Index", "Ride");
        }
    }
}