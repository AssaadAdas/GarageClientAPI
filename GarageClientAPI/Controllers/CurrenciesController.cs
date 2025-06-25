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
    public class CurrenciesController : ControllerBase
    {
        private readonly GarageClientContext _context;

        public CurrenciesController(GarageClientContext context)
        {
            _context = context;
        }

        // GET: api/Currencies
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Currency>>> GetCurrencies()
        {
            return await _context.Currencies
                .OrderBy(c => c.CurrDesc)
                .ToListAsync();
        }

        // GET: api/Currencies/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Currency>> GetCurrency(int id)
        {
            var currency = await _context.Currencies.FindAsync(id);

            if (currency == null)
            {
                return NotFound();
            }

            return currency;
        }

        // GET: api/Currencies/5/payment-orders
        [HttpGet("{id}/payment-orders")]
        public async Task<ActionResult<IEnumerable<ClientPaymentOrder>>> GetPaymentOrdersByCurrency(int id)
        {
            return await _context.ClientPaymentOrders
                .Where(o => o.Currid == id)
                .Include(o => o.PaymentMethod)
                .OrderByDescending(o => o.CreatedDate)
                .ToListAsync();
        }

        // GET: api/Currencies/5/garage-payment-orders
        [HttpGet("{id}/garage-payment-orders")]
        public async Task<ActionResult<IEnumerable<GaragePaymentOrder>>> GetGaragePaymentOrdersByCurrency(int id)
        {
            return await _context.GaragePaymentOrders
                .Where(o => o.Currid == id)
                .Include(o => o.Garage)
                .OrderByDescending(o => o.CreatedDate)
                .ToListAsync();
        }

        // GET: api/Currencies/5/premium-offers
        [HttpGet("{id}/premium-offers")]
        public async Task<ActionResult<IEnumerable<PremiumOffer>>> GetPremiumOffersByCurrency(int id)
        {
            return await _context.PremiumOffers
                .Where(o => o.CurrId == id)
                .OrderBy(o => o.PremiumCost)
                .ToListAsync();
        }

        // GET: api/Currencies/5/service-types
        [HttpGet("{id}/service-types")]
        public async Task<ActionResult<IEnumerable<VehiclesServiceType>>> GetServiceTypesByCurrency(int id)
        {
            return await _context.VehiclesServiceTypes
                .Where(s => s.CurrId == id)
                .Include(s => s.ServiceType)
                .OrderBy(s => s.ServiceType.Description)
                .ToListAsync();
        }

        // POST: api/Currencies
        [HttpPost]
        public async Task<ActionResult<Currency>> PostCurrency(Currency currency)
        {
            // Validate currency description is unique
            if (!string.IsNullOrEmpty(currency.CurrDesc) &&
                await _context.Currencies.AnyAsync(c => c.CurrDesc == currency.CurrDesc))
            {
                return Conflict("A currency with this description already exists");
            }

            _context.Currencies.Add(currency);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCurrency", new { id = currency.Id }, currency);
        }

        // PUT: api/Currencies/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCurrency(int id, Currency currency)
        {
            if (id != currency.Id)
            {
                return BadRequest();
            }

            // Validate currency description is unique (excluding current currency)
            if (!string.IsNullOrEmpty(currency.CurrDesc) &&
                await _context.Currencies.AnyAsync(c => c.CurrDesc == currency.CurrDesc && c.Id != id))
            {
                return Conflict("A currency with this description already exists");
            }

            _context.Entry(currency).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CurrencyExists(id))
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

        // DELETE: api/Currencies/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCurrency(int id)
        {
            var currency = await _context.Currencies.FindAsync(id);
            if (currency == null)
            {
                return NotFound();
            }

            // Check if currency is in use
            if (await _context.ClientPaymentOrders.AnyAsync(o => o.Currid == id) ||
                await _context.GaragePaymentOrders.AnyAsync(o => o.Currid == id) ||
                await _context.PremiumOffers.AnyAsync(o => o.CurrId == id) ||
                await _context.VehiclesServiceTypes.AnyAsync(s => s.CurrId == id))
            {
                return BadRequest("Cannot delete currency as it is being used in payment orders, premium offers, or service types");
            }

            _context.Currencies.Remove(currency);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CurrencyExists(int id)
        {
            return _context.Currencies.Any(e => e.Id == id);
        }
    }
}
