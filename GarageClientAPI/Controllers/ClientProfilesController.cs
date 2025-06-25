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
    public class ClientProfilesController : ControllerBase
    {
        private readonly GarageClientContext _context;

        public ClientProfilesController(GarageClientContext context)
        {
            _context = context;
        }

        // GET: api/ClientProfiles
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ClientProfile>>> GetClientProfiles()
        {
            return await _context.ClientProfiles
                .Include(c => c.Country)
                .Include(c => c.User)
                .Include(c => c.ClientPremiumRegistrations)
                .ToListAsync();
        }

        // GET: api/ClientProfiles/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ClientProfile>> GetClientProfile(int id)
        {
            var clientProfile = await _context.ClientProfiles
                .Include(c => c.Country)
                .Include(c => c.User)
                .Include(c => c.ClientPremiumRegistrations)
                .Include(c => c.ClientPaymentMethods)
                .Include(c => c.Vehicles)
                .Include(c => c.ClientNotifications)
                .Include(c => c.ClientReminders)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (clientProfile == null)
            {
                return NotFound();
            }

            return clientProfile;
        }

        // GET: api/ClientProfiles/GetClientProfileByUserID/5
        [HttpGet("GetClientProfileByUserID/{userid}")]
        public async Task<ActionResult<ClientProfile>> GetClientProfileByUserID(int userid)
        {
            var clientProfile = await _context.ClientProfiles
                .Include(c => c.Country)
                .Include(c => c.User)
                .Include(c => c.ClientPremiumRegistrations)
                .Include(c => c.ClientPaymentMethods)
                .Include(c => c.Vehicles)
                .Include(c => c.ClientNotifications)
                .Include(c => c.ClientReminders)
                .FirstOrDefaultAsync(c => c.User.Id == userid);

            if (clientProfile == null)
            {
                return NotFound();
            }

            return clientProfile;
        }

        // GET: api/ClientProfiles/premium
        [HttpGet("premium")]
        public async Task<ActionResult<IEnumerable<ClientProfile>>> GetPremiumClients()
        {
            return await _context.ClientProfiles
                .Where(c => c.IsPremium)
                .Include(c => c.Country)
                .Include(c => c.User)
                .ToListAsync();
        }

        // GET: api/ClientProfiles/by-email/{email}
        [HttpGet("by-email/{email}")]
        public async Task<ActionResult<ClientProfile>> GetClientByEmail(string email)
        {
            var clientProfile = await _context.ClientProfiles
                .Include(c => c.Country)
                .FirstOrDefaultAsync(c => c.Email == email);

            if (clientProfile == null)
            {
                return NotFound();
            }

            return clientProfile;
        }

        // PUT: api/ClientProfiles/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutClientProfile(int id, ClientProfile clientProfile)
        {
            if (id != clientProfile.Id)
            {
                return BadRequest();
            }

            _context.Entry(clientProfile).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ClientProfileExists(id))
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

        // POST: api/ClientProfiles
        [HttpPost]
        public async Task<ActionResult<ClientProfile>> PostClientProfile(ClientProfile clientProfile)
        {
            _context.ClientProfiles.Add(clientProfile);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetClientProfile", new { id = clientProfile.Id }, clientProfile);
        }

        // DELETE: api/ClientProfiles/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteClientProfile(int id)
        {
            var clientProfile = await _context.ClientProfiles.FindAsync(id);
            if (clientProfile == null)
            {
                return NotFound();
            }

            _context.ClientProfiles.Remove(clientProfile);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PATCH: api/ClientProfiles/5/set-premium
        [HttpPatch("{id}/set-premium")]
        public async Task<IActionResult> SetPremiumStatus(int id, [FromBody] bool isPremium)
        {
            var clientProfile = await _context.ClientProfiles.FindAsync(id);
            if (clientProfile == null)
            {
                return NotFound();
            }

            clientProfile.IsPremium = isPremium;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ClientProfileExists(int id)
        {
            return _context.ClientProfiles.Any(e => e.Id == id);
        }
    }
}
