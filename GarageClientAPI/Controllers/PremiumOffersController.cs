using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using GarageClientAPI.Data;
using GarageClientAPI.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GarageClientAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PremiumOffersController : ControllerBase
    {
        private readonly GarageClientContext _context;

        public PremiumOffersController(GarageClientContext context)
        {
            _context = context;
        }

        // GET: api/PremiumOffers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PremiumOffer>>> GetPremiumOffers()
        {
            return await _context.PremiumOffers
                .Include(p => p.Curr)
                .Include(p => p.UserType)
                .OrderBy(p => p.PremiumCost)
                .ToListAsync();
        }

        // GET: api/PremiumOffers/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PremiumOffer>> GetPremiumOffer(int id)
        {
            var premiumOffer = await _context.PremiumOffers
                .Include(p => p.Curr)
                .Include(p => p.UserType)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (premiumOffer == null)
            {
                return NotFound();
            }

            return premiumOffer;
        }

        // GET: api/PremiumOffers/type/2
        [HttpGet("type/{userTypeId}")]
        public async Task<ActionResult<IEnumerable<PremiumOffer>>> GetOffersByUserType(int userTypeId)
        {
            return await _context.PremiumOffers
                .Where(p => p.UserTypeid == userTypeId)
                .Include(p => p.Curr)
                .OrderBy(p => p.PremiumCost)
                .ToListAsync();
        }

        // GET: api/PremiumOffers/currency/1
        [HttpGet("currency/{currencyId}")]
        public async Task<ActionResult<IEnumerable<PremiumOffer>>> GetOffersByCurrency(int currencyId)
        {
            return await _context.PremiumOffers
                .Where(p => p.CurrId == currencyId)
                .Include(p => p.UserType)
                .OrderBy(p => p.PremiumCost)
                .ToListAsync();
        }

        // GET: api/PremiumOffers/active
        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<PremiumOffer>>> GetActiveOffers()
        {
            // Get offers that have been purchased at least once
            var activeOfferIds = await _context.ClientPaymentOrders
                .Select(o => o.PremiumOfferid)
                .Union(_context.GaragePaymentOrders.Select(o => o.PremiumOfferid))
                .Distinct()
                .ToListAsync();

            return await _context.PremiumOffers
                .Where(p => activeOfferIds.Contains(p.Id))
                .Include(p => p.Curr)
                .Include(p => p.UserType)
                .OrderBy(p => p.PremiumCost)
                .ToListAsync();
        }

        // POST: api/PremiumOffers
        [HttpPost]
        public async Task<ActionResult<PremiumOffer>> PostPremiumOffer(PremiumOffer premiumOffer)
        {
            // Validate unique offer description
            if (await _context.PremiumOffers.AnyAsync(p => p.PremiumDesc == premiumOffer.PremiumDesc))
            {
                return Conflict("A premium offer with this description already exists");
            }

            _context.PremiumOffers.Add(premiumOffer);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPremiumOffer", new { id = premiumOffer.Id }, premiumOffer);
        }

        // PUT: api/PremiumOffers/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPremiumOffer(int id, PremiumOffer premiumOffer)
        {
            if (id != premiumOffer.Id)
            {
                return BadRequest();
            }

            // Validate unique offer description (excluding current offer)
            if (await _context.PremiumOffers.AnyAsync(p => p.PremiumDesc == premiumOffer.PremiumDesc && p.Id != id))
            {
                return Conflict("A premium offer with this description already exists");
            }

            _context.Entry(premiumOffer).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PremiumOfferExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // PATCH: api/PremiumOffers/5/price
        [HttpPatch("{id}/price")]
        public async Task<IActionResult> UpdatePremiumPrice(int id, [FromBody] decimal newPrice)
        {
            var offer = await _context.PremiumOffers.FindAsync(id);
            if (offer == null)
            {
                return NotFound();
            }

            offer.PremiumCost = newPrice;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/PremiumOffers/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePremiumOffer(int id)
        {
            var premiumOffer = await _context.PremiumOffers.FindAsync(id);
            if (premiumOffer == null)
            {
                return NotFound();
            }

            // Check if offer is in use by any payment orders
            if (await _context.ClientPaymentOrders.AnyAsync(o => o.PremiumOfferid == id) ||
                await _context.GaragePaymentOrders.AnyAsync(o => o.PremiumOfferid == id))
            {
                return BadRequest("Cannot delete premium offer as it is being used by payment orders");
            }

            _context.PremiumOffers.Remove(premiumOffer);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("popular")]
        public async Task<ActionResult<IEnumerable<object>>> GetPopularOffers()
        {
            return await _context.PremiumOffers
                .Select(p => new
                {
                    Offer = p,
                    ClientPurchases = p.ClientPaymentOrders.Count,
                    GaragePurchases = p.GaragePaymentOrders.Count,
                    TotalPurchases = p.ClientPaymentOrders.Count + p.GaragePaymentOrders.Count
                })
                .OrderByDescending(x => x.TotalPurchases)
                .Take(5)
                .ToListAsync();
        }

        //// GET: api/PremiumOffers/5/convert/2
        //[HttpGet("{id}/convert/{targetCurrencyId}")]
        //public async Task<ActionResult<object>> GetConvertedPrice(int id, int targetCurrencyId)
        //{
        //    var offer = await _context.PremiumOffers
        //        .Include(p => p.Curr)
        //        .FirstOrDefaultAsync(p => p.Id == id);

        //    if (offer == null)
        //    {
        //        return NotFound("Offer not found");
        //    }

        //    var targetCurrency = await _context.Currencies.FindAsync(targetCurrencyId);
        //    if (targetCurrency == null)
        //    {
        //        return NotFound("Target currency not found");
        //    }

        //    // In a real implementation, you would call a currency conversion service
        //    // For this example, we'll just return the original price with a note
        //    return new
        //    {
        //        OriginalPrice = offer.PremiumCost,
        //        OriginalCurrency = offer.Curr.CurrDesc,
        //        ConvertedPrice = offer.PremiumCost, // Would be actual converted value
        //        TargetCurrency = targetCurrency.CurrDesc,
        //        Note = "Currency conversion not implemented in this example"
        //    };
        //}
        private bool PremiumOfferExists(int id)
        {
            return _context.PremiumOffers.Any(e => e.Id == id);
        }
    }
}
