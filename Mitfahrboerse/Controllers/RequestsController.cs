using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Mitfahrboerse.Models;

namespace Mitfahrboerse.Controllers
{
    public class RequestsController : Controller
    {
        private readonly MitfahrboerseDbContext _context;

        public RequestsController(MitfahrboerseDbContext context)
        {
            _context = context;
        }

        // GET: Requests
        public async Task<IActionResult> Index()
        {
            return View(await _context.t_Offers.ToListAsync());
        }

        // GET: Requests/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var t_Offer = await _context.t_Offers
                .FirstOrDefaultAsync(m => m.OfferId == id);
            if (t_Offer == null)
            {
                return NotFound();
            }

            return View(t_Offer);
        }

        // GET: Requests/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Requests/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OfferId,Title,Price,ValidUntil")] t_Offer t_Offer)
        {
            if (ModelState.IsValid)
            {
                _context.Add(t_Offer);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(t_Offer);
        }

        // GET: Requests/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var t_Offer = await _context.t_Offers.FindAsync(id);
            if (t_Offer == null)
            {
                return NotFound();
            }
            return View(t_Offer);
        }

        // POST: Requests/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("OfferId,Title,Price,ValidUntil")] t_Offer t_Offer)
        {
            if (id != t_Offer.OfferId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(t_Offer);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!t_OfferExists(t_Offer.OfferId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(t_Offer);
        }

        // GET: Requests/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var t_Offer = await _context.t_Offers
                .FirstOrDefaultAsync(m => m.OfferId == id);
            if (t_Offer == null)
            {
                return NotFound();
            }

            return View(t_Offer);
        }

        // POST: Requests/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var t_Offer = await _context.t_Offers.FindAsync(id);
            if (t_Offer != null)
            {
                _context.t_Offers.Remove(t_Offer);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool t_OfferExists(int id)
        {
            return _context.t_Offers.Any(e => e.OfferId == id);
        }
    }
}
