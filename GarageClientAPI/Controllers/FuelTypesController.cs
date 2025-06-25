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
    public class FuelTypesController : ControllerBase
    {
        private readonly GarageClientContext _context;

        public FuelTypesController(GarageClientContext context)
        {
            _context = context;
        }

        // GET: api/FuelTypes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FuelType>>> GetFuelTypes()
        {
            return await _context.FuelTypes
                .OrderBy(f => f.FuelTypeDesc)
                .ToListAsync();
        }

        // GET: api/FuelTypes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<FuelType>> GetFuelType(int id)
        {
            var fuelType = await _context.FuelTypes.FindAsync(id);

            if (fuelType == null)
            {
                return NotFound();
            }

            return fuelType;
        }

        // GET: api/FuelTypes/5/vehicles
        [HttpGet("{id}/vehicles")]
        public async Task<ActionResult<IEnumerable<Vehicle>>> GetVehiclesByFuelType(int id)
        {
            return await _context.Vehicles
                .Where(v => v.FuelTypeId == id)
                .Include(v => v.Client)
                .Include(v => v.Model)
                .OrderBy(v => v.LiscencePlate)
                .ToListAsync();
        }

        // GET: api/FuelTypes/search?term={term}
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<FuelType>>> SearchFuelTypes([FromQuery] string term)
        {
            return await _context.FuelTypes
                .Where(f => f.FuelTypeDesc.Contains(term))
                .OrderBy(f => f.FuelTypeDesc)
                .ToListAsync();
        }

        // POST: api/FuelTypes
        [HttpPost]
        public async Task<ActionResult<FuelType>> PostFuelType(FuelType fuelType)
        {
            // Validate fuel type description is unique
            if (await _context.FuelTypes.AnyAsync(f => f.FuelTypeDesc == fuelType.FuelTypeDesc))
            {
                return Conflict("A fuel type with this description already exists");
            }

            _context.FuelTypes.Add(fuelType);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetFuelType", new { id = fuelType.Id }, fuelType);
        }

        // PUT: api/FuelTypes/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutFuelType(int id, FuelType fuelType)
        {
            if (id != fuelType.Id)
            {
                return BadRequest();
            }

            // Validate fuel type description is unique (excluding current fuel type)
            if (await _context.FuelTypes.AnyAsync(f => f.FuelTypeDesc == fuelType.FuelTypeDesc && f.Id != id))
            {
                return Conflict("A fuel type with this description already exists");
            }

            _context.Entry(fuelType).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FuelTypeExists(id))
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

        // DELETE: api/FuelTypes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFuelType(int id)
        {
            var fuelType = await _context.FuelTypes.FindAsync(id);
            if (fuelType == null)
            {
                return NotFound();
            }

            // Check if fuel type is in use by any vehicles
            if (await _context.Vehicles.AnyAsync(v => v.FuelTypeId == id))
            {
                return BadRequest("Cannot delete fuel type as it is being used by vehicles");
            }

            _context.FuelTypes.Remove(fuelType);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool FuelTypeExists(int id)
        {
            return _context.FuelTypes.Any(e => e.Id == id);
        }
    }
}
