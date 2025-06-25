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
    public class GaragePremiumRegistrationsController : ControllerBase
    {
        private readonly GarageClientContext _context;

        public GaragePremiumRegistrationsController(GarageClientContext context)
        {
            _context = context;
        }

        // GET: api/GaragePremiumRegistrations
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GaragePremiumRegistration>>> GetGaragePremiumRegistrations()
        {
            return await _context.GaragePremiumRegistrations
                .Include(r => r.Garage)
                .ToListAsync();
        }

        // GET: api/GaragePremiumRegistrations/5
        [HttpGet("{id}")]
        public async Task<ActionResult<GaragePremiumRegistration>> GetGaragePremiumRegistration(int id)
        {
            var registration = await _context.GaragePremiumRegistrations
                .Include(r => r.Garage)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (registration == null)
            {
                return NotFound();
            }

            return registration;
        }

        // GET: api/GaragePremiumRegistrations/garage/5
        [HttpGet("garage/{garageId}")]
        public async Task<ActionResult<IEnumerable<GaragePremiumRegistration>>> GetRegistrationsByGarage(int garageId)
        {
            return await _context.GaragePremiumRegistrations
                .Where(r => r.Garageid == garageId)
                .Include(r => r.Garage)
                .OrderByDescending(r => r.Registerdate)
                .ToListAsync();
        }

        // GET: api/GaragePremiumRegistrations/active
        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<GaragePremiumRegistration>>> GetActiveRegistrations()
        {
            var now = DateTime.Now;
            return await _context.GaragePremiumRegistrations
                .Where(r => r.IsActive && r.ExpiryDate >= now)
                .Include(r => r.Garage)
                .ToListAsync();
        }

        // POST: api/GaragePremiumRegistrations
        [HttpPost]
        public async Task<ActionResult<GaragePremiumRegistration>> PostGaragePremiumRegistration(GaragePremiumRegistration registration)
        {
            // Set default registration date if not provided
            if (registration.Registerdate == default)
            {
                registration.Registerdate = DateTime.Now;
            }

            // Deactivate any other active registrations for this garage
            var existingActive = await _context.GaragePremiumRegistrations
                .Where(r => r.Garageid == registration.Garageid && r.IsActive)
                .ToListAsync();

            foreach (var activeReg in existingActive)
            {
                activeReg.IsActive = false;
            }

            _context.GaragePremiumRegistrations.Add(registration);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetGaragePremiumRegistration", new { id = registration.Id }, registration);
        }

        // PUT: api/GaragePremiumRegistrations/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutGaragePremiumRegistration(int id, GaragePremiumRegistration registration)
        {
            if (id != registration.Id)
            {
                return BadRequest();
            }

            _context.Entry(registration).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!GaragePremiumRegistrationExists(id))
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

        // PATCH: api/GaragePremiumRegistrations/5/activate
        [HttpPatch("{id}/activate")]
        public async Task<IActionResult> ActivateRegistration(int id)
        {
            var registration = await _context.GaragePremiumRegistrations.FindAsync(id);
            if (registration == null)
            {
                return NotFound();
            }

            // Deactivate all other registrations for this garage
            var garageRegistrations = await _context.GaragePremiumRegistrations
                .Where(r => r.Garageid == registration.Garageid && r.Id != id)
                .ToListAsync();

            foreach (var reg in garageRegistrations)
            {
                reg.IsActive = false;
            }

            registration.IsActive = true;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PATCH: api/GaragePremiumRegistrations/5/extend
        [HttpPatch("{id}/extend")]
        public async Task<IActionResult> ExtendRegistration(int id, [FromBody] int monthsToExtend)
        {
            var registration = await _context.GaragePremiumRegistrations.FindAsync(id);
            if (registration == null)
            {
                return NotFound();
            }

            // Use current date if expiry is in the past
            var baseDate = registration.ExpiryDate > DateTime.Now ? registration.ExpiryDate : DateTime.Now;
            registration.ExpiryDate = baseDate.AddMonths(monthsToExtend);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/GaragePremiumRegistrations/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGaragePremiumRegistration(int id)
        {
            var registration = await _context.GaragePremiumRegistrations.FindAsync(id);
            if (registration == null)
            {
                return NotFound();
            }

            _context.GaragePremiumRegistrations.Remove(registration);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool GaragePremiumRegistrationExists(int id)
        {
            return _context.GaragePremiumRegistrations.Any(e => e.Id == id);
        }
    }
}
