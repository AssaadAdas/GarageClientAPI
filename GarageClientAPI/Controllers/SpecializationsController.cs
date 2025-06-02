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
    public class SpecializationsController : ControllerBase
    {
        private readonly GarageClientContext _context;

        public SpecializationsController(GarageClientContext context)
        {
            _context = context;
        }

        // GET: api/Specializations
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Specialization>>> GetSpecializations()
        {
            return await _context.Specializations
                .OrderBy(s => s.SpecializationDesc)
                .ToListAsync();
        }

        // GET: api/Specializations/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Specialization>> GetSpecialization(int id)
        {
            var specialization = await _context.Specializations
                .Include(s => s.GarageProfiles)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (specialization == null)
            {
                return NotFound();
            }

            return specialization;
        }

        // GET: api/Specializations/search?term=auto
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<Specialization>>> SearchSpecializations([FromQuery] string term)
        {
            return await _context.Specializations
                .Where(s => s.SpecializationDesc.Contains(term))
                .OrderBy(s => s.SpecializationDesc)
                .ToListAsync();
        }

        // GET: api/Specializations/5/garages
        [HttpGet("{id}/garages")]
        public async Task<ActionResult<IEnumerable<GarageProfile>>> GetGaragesBySpecialization(int id)
        {
            return await _context.GarageProfiles
                .Where(g => g.SpecializationId == id)
                .Include(g => g.Country)
                .Include(g => g.User)
                .OrderBy(g => g.GarageName)
                .ToListAsync();
        }

        // POST: api/Specializations
        [HttpPost]
        public async Task<ActionResult<Specialization>> PostSpecialization(Specialization specialization)
        {
            // Validate description is unique
            if (await _context.Specializations.AnyAsync(s => s.SpecializationDesc == specialization.SpecializationDesc))
            {
                return Conflict("A specialization with this description already exists");
            }

            _context.Specializations.Add(specialization);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetSpecialization", new { id = specialization.Id }, specialization);
        }

        // PUT: api/Specializations/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSpecialization(int id, Specialization specialization)
        {
            if (id != specialization.Id)
            {
                return BadRequest();
            }

            // Validate description is unique (excluding current specialization)
            if (await _context.Specializations.AnyAsync(s =>
                s.SpecializationDesc == specialization.SpecializationDesc &&
                s.Id != id))
            {
                return Conflict("A specialization with this description already exists");
            }

            _context.Entry(specialization).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SpecializationExists(id))
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

        // DELETE: api/Specializations/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSpecialization(int id)
        {
            var specialization = await _context.Specializations.FindAsync(id);
            if (specialization == null)
            {
                return NotFound();
            }

            // Check if specialization is in use by any garages
            if (await _context.GarageProfiles.AnyAsync(g => g.SpecializationId == id))
            {
                return BadRequest("Cannot delete specialization as it is being used by garages");
            }

            _context.Specializations.Remove(specialization);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool SpecializationExists(int id)
        {
            return _context.Specializations.Any(e => e.Id == id);
        }
    }
}
