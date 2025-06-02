using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GarageClientAPI.Data;
using GarageClientAPI.Models;

namespace GarageClientAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientPremiumRegistrationsController : ControllerBase
    {
        private readonly GarageClientContext _context;

        public ClientPremiumRegistrationsController(GarageClientContext context)
        {
            _context = context;
        }

        // GET: api/ClientPremiumRegistrations
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ClientPremiumRegistration>>> GetClientPremiumRegistrations()
        {
            return await _context.ClientPremiumRegistrations.Include(c => c.Client).ToListAsync();
        }

        // GET: api/ClientPremiumRegistrations/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ClientPremiumRegistration>> GetClientPremiumRegistration(int id)
        {
            var clientPremiumRegistration = await _context.ClientPremiumRegistrations
                .Include(c => c.Client)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (clientPremiumRegistration == null)
            {
                return NotFound();
            }

            return clientPremiumRegistration;
        }

        // GET: api/ClientPremiumRegistrations/client/5
        [HttpGet("client/{clientId}")]
        public async Task<ActionResult<IEnumerable<ClientPremiumRegistration>>> GetRegistrationsByClient(int clientId)
        {
            return await _context.ClientPremiumRegistrations
                .Where(c => c.Clientid == clientId)
                .Include(c => c.Client)
                .ToListAsync();
        }

        // GET: api/ClientPremiumRegistrations/active
        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<ClientPremiumRegistration>>> GetActiveRegistrations()
        {
            return await _context.ClientPremiumRegistrations
                .Where(c => c.IsActive && c.ExpiryDate >= DateTime.Now)
                .Include(c => c.Client)
                .ToListAsync();
        }

        // PUT: api/ClientPremiumRegistrations/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutClientPremiumRegistration(int id, ClientPremiumRegistration clientPremiumRegistration)
        {
            if (id != clientPremiumRegistration.Id)
            {
                return BadRequest();
            }

            _context.Entry(clientPremiumRegistration).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ClientPremiumRegistrationExists(id))
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

        // POST: api/ClientPremiumRegistrations
        [HttpPost]
        public async Task<ActionResult<ClientPremiumRegistration>> PostClientPremiumRegistration(ClientPremiumRegistration clientPremiumRegistration)
        {
            // Set default values if needed
            if (clientPremiumRegistration.Registerdate == default)
            {
                clientPremiumRegistration.Registerdate = DateTime.Now;
            }

            _context.ClientPremiumRegistrations.Add(clientPremiumRegistration);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetClientPremiumRegistration", new { id = clientPremiumRegistration.Id }, clientPremiumRegistration);
        }

        // DELETE: api/ClientPremiumRegistrations/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteClientPremiumRegistration(int id)
        {
            var clientPremiumRegistration = await _context.ClientPremiumRegistrations.FindAsync(id);
            if (clientPremiumRegistration == null)
            {
                return NotFound();
            }

            _context.ClientPremiumRegistrations.Remove(clientPremiumRegistration);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ClientPremiumRegistrationExists(int id)
        {
            return _context.ClientPremiumRegistrations.Any(e => e.Id == id);
        }
    }
}
