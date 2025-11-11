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
    public class GaragePaymentMethodsController : ControllerBase
    {
        private readonly GarageClientContext _context;

        public GaragePaymentMethodsController(GarageClientContext context)
        {
            _context = context;
        }

        // GET: api/GaragePaymentMethods
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GaragePaymentMethod>>> GetGaragePaymentMethods()
        {
            return await _context.GaragePaymentMethods
                .Include(p => p.Garage)
                .ToListAsync();
        }

        // GET: api/GaragePaymentMethods/5
        [HttpGet("{id}")]
        public async Task<ActionResult<GaragePaymentMethod>> GetGaragePaymentMethod(int id)
        {
            var paymentMethod = await _context.GaragePaymentMethods
                .Include(p => p.Garage)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (paymentMethod == null)
            {
                return NotFound();
            }

            // Mask sensitive card details in response
            var response = new
            {
                paymentMethod.Id,
                paymentMethod.Garageid,
                paymentMethod.PaymentType,
                paymentMethod.IsPrimary,
                paymentMethod.CreatedDate,
                paymentMethod.LastModified,
                paymentMethod.IsActive,
                CardNumber = MaskCardNumber(paymentMethod.CardNumber),
                paymentMethod.CardHolderName,
                paymentMethod.ExpiryMonth,
                paymentMethod.ExpiryYear,
                Garage = paymentMethod.Garage
            };

            return Ok(response);
        }
        // GET: api/GaragePaymentMethods/5
        [HttpGet("UnMask/{id}")]
        public async Task<ActionResult<GaragePaymentMethod>> GetGaragePaymentMethodUnMask(int id)
        {
            var paymentMethod = await _context.GaragePaymentMethods
                .Include(p => p.Garage)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (paymentMethod == null)
            {
                return NotFound();
            }

            // Mask sensitive card details in response
            var response = new
            {
                paymentMethod.Id,
                paymentMethod.Garageid,
                paymentMethod.PaymentType,
                paymentMethod.IsPrimary,
                paymentMethod.CreatedDate,
                paymentMethod.LastModified,
                paymentMethod.IsActive,
                CardNumber = paymentMethod.CardNumber,
                paymentMethod.CardHolderName,
                paymentMethod.ExpiryMonth,
                paymentMethod.ExpiryYear,
                Garage = paymentMethod.Garage
            };

            return Ok(response);
        }
        // GET: api/GaragePaymentMethods/garage/5
        [HttpGet("garage/{garageId}")]
        public async Task<ActionResult<IEnumerable<GaragePaymentMethod>>> GetPaymentMethodsByGarage(int garageId)
        {
            var methods = await _context.GaragePaymentMethods
                .Where(p => p.Garageid == garageId)
                .Include(p => p.Garage)
                .ToListAsync();

            // Mask sensitive card details for all methods
            var response = methods.Select(m => new
            {
                m.Id,
                m.Garageid,
                m.PaymentType,
                m.IsPrimary,
                m.CreatedDate,
                m.LastModified,
                m.IsActive,
                CardNumber = MaskCardNumber(m.CardNumber),
                m.CardHolderName,
                m.ExpiryMonth,
                m.ExpiryYear,
                Garage = m.Garage
            });

            return Ok(response);
        }

        // GET: api/GaragePaymentMethods/garage/5/primary
        [HttpGet("garage/{garageId}/primary")]
        public async Task<ActionResult<GaragePaymentMethod>> GetPrimaryPaymentMethod(int garageId)
        {
            var method = await _context.GaragePaymentMethods
                .Where(p => p.Garageid == garageId && p.IsPrimary)
                .Include(p => p.Garage)
                .FirstOrDefaultAsync();

            if (method == null)
            {
                return NotFound();
            }

            // Mask sensitive card details
            var response = new
            {
                method.Id,
                method.Garageid,
                method.PaymentType,
                method.IsPrimary,
                method.CreatedDate,
                method.LastModified,
                method.IsActive,
                CardNumber = MaskCardNumber(method.CardNumber),
                method.CardHolderName,
                method.ExpiryMonth,
                method.ExpiryYear,
                Garage = method.Garage
            };

            return Ok(response);
        }

        // POST: api/GaragePaymentMethods
        [HttpPost]
        public async Task<ActionResult<GaragePaymentMethod>> PostGaragePaymentMethod(GaragePaymentMethod paymentMethod)
        {
            // Set default dates
            paymentMethod.CreatedDate = DateTime.Now;
            paymentMethod.LastModified = DateTime.Now;

            // If this is the first payment method, set as primary
            if (!await _context.GaragePaymentMethods.AnyAsync(p => p.Garageid == paymentMethod.Garageid))
            {
                paymentMethod.IsPrimary = true;
            }

            _context.GaragePaymentMethods.Add(paymentMethod);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetGaragePaymentMethod", new { id = paymentMethod.Id }, paymentMethod);
        }

        // PUT: api/GaragePaymentMethods/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutGaragePaymentMethod(int id, GaragePaymentMethod paymentMethod)
        {
            if (id != paymentMethod.Id)
            {
                return BadRequest();
            }

            // Update last modified date
            paymentMethod.LastModified = DateTime.Now;

            _context.Entry(paymentMethod).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!GaragePaymentMethodExists(id))
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

        // PATCH: api/GaragePaymentMethods/5/set-primary
        [HttpPatch("{id}/set-primary")]
        public async Task<IActionResult> SetPrimaryPaymentMethod(int id)
        {
            var paymentMethod = await _context.GaragePaymentMethods.FindAsync(id);
            if (paymentMethod == null)
            {
                return NotFound();
            }

            // Reset all primary methods for this garage
            var garageMethods = await _context.GaragePaymentMethods
                .Where(p => p.Garageid == paymentMethod.Garageid)
                .ToListAsync();

            foreach (var method in garageMethods)
            {
                method.IsPrimary = false;
            }

            // Set this method as primary
            paymentMethod.IsPrimary = true;
            paymentMethod.LastModified = DateTime.Now;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/GaragePaymentMethods/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGaragePaymentMethod(int id)
        {
            var paymentMethod = await _context.GaragePaymentMethods.FindAsync(id);
            if (paymentMethod == null)
            {
                return NotFound();
            }

            // If deleting primary method, assign a new primary if others exist
            if (paymentMethod.IsPrimary)
            {
                var otherMethods = await _context.GaragePaymentMethods
                    .Where(p => p.Garageid == paymentMethod.Garageid && p.Id != id)
                    .ToListAsync();

                if (otherMethods.Any())
                {
                    otherMethods.First().IsPrimary = true;
                    otherMethods.First().LastModified = DateTime.Now;
                }
            }

            _context.GaragePaymentMethods.Remove(paymentMethod);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool GaragePaymentMethodExists(int id)
        {
            return _context.GaragePaymentMethods.Any(e => e.Id == id);
        }

        private string MaskCardNumber(string cardNumber)
        {
            if (string.IsNullOrEmpty(cardNumber) || cardNumber.Length < 4)
            {
                return "****";
            }

            return new string('*', cardNumber.Length - 4) + cardNumber.Substring(cardNumber.Length - 4);
        }
    }
}
