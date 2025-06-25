using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using GarageClientAPI.Data;
using GarageClientAPI.Models;
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
    public class MeassureUnitsController : ControllerBase
    {
        private readonly GarageClientContext _context;

        public MeassureUnitsController(GarageClientContext context)
        {
            _context = context;
        }

        // GET: api/MeassureUnits
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MeassureUnit>>> GetMeassureUnits()
        {
            return await _context.MeassureUnits
                .OrderBy(m => m.MeassureUnitDesc)
                .ToListAsync();
        }

        // GET: api/MeassureUnits/5
        [HttpGet("{id}")]
        public async Task<ActionResult<MeassureUnit>> GetMeassureUnit(int id)
        {
            var meassureUnit = await _context.MeassureUnits
                .Include(m => m.ServicesTypeSetUps)
                .Include(m => m.Vehicles)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (meassureUnit == null)
            {
                return NotFound();
            }

            return meassureUnit;
        }

        // GET: api/MeassureUnits/search?term={term}
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<MeassureUnit>>> SearchMeassureUnits([FromQuery] string term)
        {
            return await _context.MeassureUnits
                .Where(m => m.MeassureUnitDesc.Contains(term))
                .OrderBy(m => m.MeassureUnitDesc)
                .ToListAsync();
        }

        // GET: api/MeassureUnits/5/services
        [HttpGet("{id}/services")]
        public async Task<ActionResult<IEnumerable<ServicesTypeSetUp>>> GetServicesUsingMeassureUnit(int id)
        {
            return await _context.ServicesTypeSetUps
                .Where(s => s.MeassureUnitid == id)
                .Include(s => s.ServiceTypes)
                .OrderBy(s => s.ServiceTypes.Description)
                .ToListAsync();
        }

        // GET: api/MeassureUnits/5/vehicles
        [HttpGet("{id}/vehicles")]
        public async Task<ActionResult<IEnumerable<Vehicle>>> GetVehiclesUsingMeassureUnit(int id)
        {
            return await _context.Vehicles
                .Where(v => v.MeassureUnitId == id)
                .Include(v => v.Client)
                .Include(v => v.Model)
                .OrderBy(v => v.LiscencePlate)
                .ToListAsync();
        }

        // POST: api/MeassureUnits
        [HttpPost]
        public async Task<ActionResult<MeassureUnit>> PostMeassureUnit(MeassureUnit meassureUnit)
        {
            // Validate description is unique
            if (await _context.MeassureUnits.AnyAsync(m => m.MeassureUnitDesc == meassureUnit.MeassureUnitDesc))
            {
                return Conflict("A measurement unit with this description already exists");
            }

            _context.MeassureUnits.Add(meassureUnit);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetMeassureUnit", new { id = meassureUnit.Id }, meassureUnit);
        }

        // PUT: api/MeassureUnits/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMeassureUnit(int id, MeassureUnit meassureUnit)
        {
            if (id != meassureUnit.Id)
            {
                return BadRequest();
            }

            // Validate description is unique (excluding current unit)
            if (await _context.MeassureUnits.AnyAsync(m => m.MeassureUnitDesc == meassureUnit.MeassureUnitDesc && m.Id != id))
            {
                return Conflict("A measurement unit with this description already exists");
            }

            _context.Entry(meassureUnit).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MeassureUnitExists(id))
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

        // DELETE: api/MeassureUnits/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMeassureUnit(int id)
        {
            var meassureUnit = await _context.MeassureUnits.FindAsync(id);
            if (meassureUnit == null)
            {
                return NotFound();
            }

            // Check if unit is in use by services or vehicles
            if (await _context.ServicesTypeSetUps.AnyAsync(s => s.MeassureUnitid == id) ||
                await _context.Vehicles.AnyAsync(v => v.MeassureUnitId == id))
            {
                return BadRequest("Cannot delete measurement unit as it is being used by services or vehicles");
            }

            _context.MeassureUnits.Remove(meassureUnit);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool MeassureUnitExists(int id)
        {
            return _context.MeassureUnits.Any(e => e.Id == id);
        }
    }
}
