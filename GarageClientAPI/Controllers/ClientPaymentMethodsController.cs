using GarageClientAPI.Data;
using GarageClientAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace GarageClientAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientPaymentMethodsController : ControllerBase
    {
        private readonly GarageClientContext _context;

        public ClientPaymentMethodsController(GarageClientContext context)
        {
            _context = context;
        }

        // GET: api/ClientPaymentMethods
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ClientPaymentMethod>>> GetClientPaymentMethods()
        {
            return await _context.ClientPaymentMethods
                .Include(c => c.Client)
                .ToListAsync();
        }

        // GET: api/ClientPaymentMethods/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ClientPaymentMethod>> GetClientPaymentMethod(int id)
        {
            var clientPaymentMethod = await _context.ClientPaymentMethods
                .Include(c => c.Client)
                .Include(c => c.ClientPaymentOrders)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (clientPaymentMethod == null)
            {
                return NotFound();
            }

            // Mask sensitive card details in response
            var response = new
            {
                clientPaymentMethod.Id,
                clientPaymentMethod.Clientid,
                clientPaymentMethod.PaymentType,
                clientPaymentMethod.IsPrimary,
                clientPaymentMethod.CreatedDate,
                clientPaymentMethod.LastModified,
                clientPaymentMethod.IsActive,
                CardNumber = MaskCardNumber(clientPaymentMethod.CardNumber),
                clientPaymentMethod.CardHolderName,
                clientPaymentMethod.ExpiryMonth,
                clientPaymentMethod.ExpiryYear,
                Client = clientPaymentMethod.Client,
                ClientPaymentOrders = clientPaymentMethod.ClientPaymentOrders
            };

            return Ok(response);
        }

        // GET: api/ClientPaymentMethods/client/5
        [HttpGet("client/{clientId}")]
        public async Task<ActionResult<IEnumerable<ClientPaymentMethod>>> GetPaymentMethodsByClient(int clientId)
        {
            var methods = await _context.ClientPaymentMethods
                .Where(c => c.Clientid == clientId)
                .Include(c => c.Client)
                .ToListAsync();

            // Mask sensitive card details for all methods
            var response = methods.Select(m => new
            {
                m.Id,
                m.Clientid,
                m.PaymentType,
                m.IsPrimary,
                m.CreatedDate,
                m.LastModified,
                m.IsActive,
                CardNumber = MaskCardNumber(m.CardNumber),
                m.CardHolderName,
                m.ExpiryMonth,
                m.ExpiryYear,
                Client = m.Client
            });

            return Ok(response);
        }

        // GET: api/ClientPaymentMethods/client/5/primary
        [HttpGet("client/{clientId}/primary")]
        public async Task<ActionResult<ClientPaymentMethod>> GetPrimaryPaymentMethod(int clientId)
        {
            var method = await _context.ClientPaymentMethods
                .Where(c => c.Clientid == clientId && c.IsPrimary)
                .Include(c => c.Client)
                .FirstOrDefaultAsync();

            if (method == null)
            {
                return NotFound();
            }

            // Mask sensitive card details
            var response = new
            {
                method.Id,
                method.Clientid,
                method.PaymentType,
                method.IsPrimary,
                method.CreatedDate,
                method.LastModified,
                method.IsActive,
                CardNumber = MaskCardNumber(method.CardNumber),
                method.CardHolderName,
                method.ExpiryMonth,
                method.ExpiryYear,
                Client = method.Client
            };

            return Ok(response);
        }

        // POST: api/ClientPaymentMethods
        [HttpPost]
        public async Task<ActionResult<ClientPaymentMethod>> PostClientPaymentMethod(ClientPaymentMethod clientPaymentMethod)
        {
            // Set default dates
            clientPaymentMethod.CreatedDate = DateTime.Now;
            clientPaymentMethod.LastModified = DateTime.Now;

            // If this is the first payment method, set as primary
            if (!await _context.ClientPaymentMethods.AnyAsync(c => c.Clientid == clientPaymentMethod.Clientid))
            {
                clientPaymentMethod.IsPrimary = true;
            }

            _context.ClientPaymentMethods.Add(clientPaymentMethod);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetClientPaymentMethod", new { id = clientPaymentMethod.Id }, clientPaymentMethod);
        }

        // PUT: api/ClientPaymentMethods/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutClientPaymentMethod(int id, ClientPaymentMethod clientPaymentMethod)
        {
            if (id != clientPaymentMethod.Id)
            {
                return BadRequest();
            }

            // Update last modified date
            clientPaymentMethod.LastModified = DateTime.Now;

            _context.Entry(clientPaymentMethod).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ClientPaymentMethodExists(id))
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

        // PATCH: api/ClientPaymentMethods/5/set-primary
        [HttpPatch("{id}/set-primary")]
        public async Task<IActionResult> SetPrimaryPaymentMethod(int id)
        {
            var paymentMethod = await _context.ClientPaymentMethods.FindAsync(id);
            if (paymentMethod == null)
            {
                return NotFound();
            }

            // Reset all primary methods for this client
            var clientMethods = await _context.ClientPaymentMethods
                .Where(c => c.Clientid == paymentMethod.Clientid)
                .ToListAsync();

            foreach (var method in clientMethods)
            {
                method.IsPrimary = false;
            }

            // Set this method as primary
            paymentMethod.IsPrimary = true;
            paymentMethod.LastModified = DateTime.Now;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/ClientPaymentMethods/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteClientPaymentMethod(int id)
        {
            var clientPaymentMethod = await _context.ClientPaymentMethods.FindAsync(id);
            if (clientPaymentMethod == null)
            {
                return NotFound();
            }

            // If deleting primary method, assign a new primary if others exist
            if (clientPaymentMethod.IsPrimary)
            {
                var otherMethods = await _context.ClientPaymentMethods
                    .Where(c => c.Clientid == clientPaymentMethod.Clientid && c.Id != id)
                    .ToListAsync();

                if (otherMethods.Any())
                {
                    otherMethods.First().IsPrimary = true;
                    otherMethods.First().LastModified = DateTime.Now;
                }
            }

            _context.ClientPaymentMethods.Remove(clientPaymentMethod);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ClientPaymentMethodExists(int id)
        {
            return _context.ClientPaymentMethods.Any(e => e.Id == id);
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
