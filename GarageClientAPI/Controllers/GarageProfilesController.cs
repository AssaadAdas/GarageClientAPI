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
    public class GarageProfilesController : ControllerBase
    {
        private readonly GarageClientContext _context;

        public GarageProfilesController(GarageClientContext context)
        {
            _context = context;
        }

        // GET: api/GarageProfiles
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GarageProfile>>> GetGarageProfiles()
        {
            return await _context.GarageProfiles
                .Include(g => g.Country)
                .Include(g => g.Specialization)
                .Include(g => g.User)
                .ToListAsync();
        }

        // GET: api/ClientProfiles/GetClientProfileByUserID/5
        [HttpGet("GetGarageProfileByUserID/{userid}")]
        public async Task<ActionResult<GarageProfile>> GetGarageProfileByUserID(int userid)
        {
            var GarageProfile = await _context.GarageProfiles
                .Include(c => c.Country)
                .Include(c => c.User)
                .Include(c => c.GaragePremiumRegistrations)
                .Include(c => c.GaragePaymentMethods)
                .FirstOrDefaultAsync(c => c.User.Id == userid);

            if (GarageProfile == null)
            {
                return NotFound();
            }

            return GarageProfile;
        }
        // GET: api/GarageProfiles/5
        [HttpGet("{id}")]
        public async Task<ActionResult<GarageProfile>> GetGarageProfile(int id)
        {
            var garageProfile = await _context.GarageProfiles
                .Include(g => g.Country)
                .Include(g => g.Specialization)
                .Include(g => g.User)
                .Include(g => g.GaragePaymentMethods)
                .Include(g => g.GaragePaymentOrders)
                .Include(g => g.GaragePremiumRegistrations)
                .Include(g => g.VehiclesServices)
                .ThenInclude(vs=> vs.Vehicle)
                .ThenInclude(v=>v.Client)
                .Include(g => g.VehicleAppointments)
                .ThenInclude(va => va.Vehicle)
                .ThenInclude(v => v.Client)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (garageProfile == null)
            {
                return NotFound();
            }

            return garageProfile;
        }

        // GET: api/GarageProfiles/premium
        [HttpGet("premium")]
        public async Task<ActionResult<IEnumerable<GarageProfile>>> GetPremiumGarages()
        {
            return await _context.GarageProfiles
                .Where(g => g.IsPremium)
                .Include(g => g.Country)
                .Include(g => g.Specialization)
                .ToListAsync();
        }

        // GET: api/GarageProfiles/country/5
        [HttpGet("country/{countryId}")]
        public async Task<ActionResult<IEnumerable<GarageProfile>>> GetGaragesByCountry(int countryId)
        {
            return await _context.GarageProfiles
                .Where(g => g.CountryId == countryId)
                .Include(g => g.Specialization)
                .OrderBy(g => g.GarageName)
                .ToListAsync();
        }

        // GET: api/GarageProfiles/specialization/5
        [HttpGet("specialization/{specializationId}")]
        public async Task<ActionResult<IEnumerable<GarageProfile>>> GetGaragesBySpecialization(int specializationId)
        {
            return await _context.GarageProfiles
                .Where(g => g.SpecializationId == specializationId)
                .Include(g => g.Country)
                .OrderBy(g => g.GarageName)
                .ToListAsync();
        }

        // GET: api/GarageProfiles/search?query={query}
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<GarageProfile>>> SearchGarages([FromQuery] string query)
        {
            return await _context.GarageProfiles
                .Where(g => g.GarageName.Contains(query) ||
                           g.Address.Contains(query) ||
                           (g.Email != null && g.Email.Contains(query)))
                .Include(g => g.Country)
                .Include(g => g.Specialization)
                .OrderBy(g => g.GarageName)
                .ToListAsync();
        }

        // POST: api/GarageProfiles
        [HttpPost]
        public async Task<ActionResult<GarageProfile>> PostGarageProfile(GarageProfile garageProfile)
        {
            // Validate email is unique if provided
            if (!string.IsNullOrEmpty(garageProfile.Email) &&
                await _context.GarageProfiles.AnyAsync(g => g.Email == garageProfile.Email))
            {
                return Conflict("A garage with this email already exists");
            }

            _context.GarageProfiles.Add(garageProfile);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetGarageProfile", new { id = garageProfile.Id }, garageProfile);
        }

        // PUT: api/GarageProfiles/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutGarageProfile(int id, GarageProfile garageProfile)
        {
            if (id != garageProfile.Id)
            {
                return BadRequest();
            }

            // Validate email is unique if provided (excluding current garage)
            if (!string.IsNullOrEmpty(garageProfile.Email) &&
                await _context.GarageProfiles.AnyAsync(g => g.Email == garageProfile.Email && g.Id != id))
            {
                return Conflict("A garage with this email already exists");
            }

            _context.Entry(garageProfile).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!GarageProfileExists(id))
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

        // PATCH: api/GarageProfiles/5/premium-status
        [HttpPatch("{id}/premium-status")]
        public async Task<IActionResult> UpdatePremiumStatus(int id, [FromBody] bool isPremium)
        {
            var garage = await _context.GarageProfiles.FindAsync(id);
            if (garage == null)
            {
                return NotFound();
            }

            garage.IsPremium = isPremium;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/GarageProfiles/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGarageProfile(int id)
        {
            var garageProfile = await _context.GarageProfiles.FindAsync(id);
            if (garageProfile == null)
            {
                return NotFound();
            }

            // Check for dependent records
            if (await _context.GaragePaymentMethods.AnyAsync(p => p.Garageid == id) ||
                await _context.GaragePaymentOrders.AnyAsync(o => o.GarageId == id) ||
                await _context.VehiclesServices.AnyAsync(s => s.Garageid == id))
            {
                return BadRequest("Cannot delete garage as it has associated payment methods, orders, or services");
            }

            _context.GarageProfiles.Remove(garageProfile);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        // GET: api/GarageProfiles/5/check-premium
        [HttpGet("{id}/check-premium")]
        public async Task<ActionResult<bool>> CheckPremiumStatus(int id)
        {
            var garage = await _context.GarageProfiles.FindAsync(id);
            if (garage == null)
            {
                return NotFound();
            }

            var hasActivePremium = await _context.GaragePremiumRegistrations
                .AnyAsync(r => r.Garageid == id && r.IsActive && r.ExpiryDate >= DateTime.Now);

            if (garage.IsPremium != hasActivePremium)
            {
                garage.IsPremium = hasActivePremium;
                await _context.SaveChangesAsync();
            }

            return hasActivePremium;
        }
        private bool GarageProfileExists(int id)
        {
            return _context.GarageProfiles.Any(e => e.Id == id);
        }
    }
}
