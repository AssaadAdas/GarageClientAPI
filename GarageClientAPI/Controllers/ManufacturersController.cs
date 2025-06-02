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
    public class ManufacturersController : ControllerBase
    {
        private readonly GarageClientContext _context;

        public ManufacturersController(GarageClientContext context)
        {
            _context = context;
        }

        // GET: api/Manufacturers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Manufacturer>>> GetManufacturers()
        {
            return await _context.Manufacturers
                .OrderBy(m => m.ManufacturerDesc)
                .ToListAsync();
        }

        // GET: api/Manufacturers/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Manufacturer>> GetManufacturer(int id)
        {
            var manufacturer = await _context.Manufacturers
                .Include(m => m.Vehicles)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (manufacturer == null)
            {
                return NotFound();
            }

            return manufacturer;
        }

        // GET: api/Manufacturers/search?term={term}
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<Manufacturer>>> SearchManufacturers([FromQuery] string term)
        {
            return await _context.Manufacturers
                .Where(m => m.ManufacturerDesc.Contains(term))
                .OrderBy(m => m.ManufacturerDesc)
                .ToListAsync();
        }

        // GET: api/Manufacturers/5/vehicles
        [HttpGet("{id}/vehicles")]
        public async Task<ActionResult<IEnumerable<Vehicle>>> GetVehiclesByManufacturer(int id)
        {
            return await _context.Vehicles
                .Where(v => v.ManufacturerId == id)
                .Include(v => v.Client)
                .Include(v => v.Model)
                .OrderBy(v => v.LiscencePlate)
                .ToListAsync();
        }

        // GET: api/Manufacturers/5/logo
        [HttpGet("{id}/logo")]
        public async Task<IActionResult> GetManufacturerLogo(int id)
        {
            var manufacturer = await _context.Manufacturers.FindAsync(id);
            if (manufacturer == null || manufacturer.ManufacturerLogo == null)
            {
                return NotFound();
            }

            return File(manufacturer.ManufacturerLogo, "image/png"); // Adjust content type based on your image format
        }

        // POST: api/Manufacturers
        [HttpPost]
        public async Task<ActionResult<Manufacturer>> PostManufacturer(Manufacturer manufacturer)
        {
            // Validate manufacturer description is unique
            if (await _context.Manufacturers.AnyAsync(m => m.ManufacturerDesc == manufacturer.ManufacturerDesc))
            {
                return Conflict("A manufacturer with this description already exists");
            }

            _context.Manufacturers.Add(manufacturer);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetManufacturer", new { id = manufacturer.Id }, manufacturer);
        }

        // PUT: api/Manufacturers/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutManufacturer(int id, Manufacturer manufacturer)
        {
            if (id != manufacturer.Id)
            {
                return BadRequest();
            }

            // Validate manufacturer description is unique (excluding current manufacturer)
            if (await _context.Manufacturers.AnyAsync(m => m.ManufacturerDesc == manufacturer.ManufacturerDesc && m.Id != id))
            {
                return Conflict("A manufacturer with this description already exists");
            }

            _context.Entry(manufacturer).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ManufacturerExists(id))
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

        // PATCH: api/Manufacturers/5/logo
        [HttpPatch("{id}/logo")]
        public async Task<IActionResult> UpdateManufacturerLogo(int id, [FromBody] byte[] logo)
        {
            var manufacturer = await _context.Manufacturers.FindAsync(id);
            if (manufacturer == null)
            {
                return NotFound();
            }

            manufacturer.ManufacturerLogo = logo;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/Manufacturers/5/logo/upload
        [HttpPost("{id}/logo/upload")]
        public async Task<IActionResult> UploadManufacturerLogo(int id, IFormFile file)
        {
            var manufacturer = await _context.Manufacturers.FindAsync(id);
            if (manufacturer == null)
            {
                return NotFound();
            }

            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded");
            }

            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                manufacturer.ManufacturerLogo = memoryStream.ToArray();
            }

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Manufacturers/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteManufacturer(int id)
        {
            var manufacturer = await _context.Manufacturers.FindAsync(id);
            if (manufacturer == null)
            {
                return NotFound();
            }

            // Check if manufacturer is in use by any vehicles
            if (await _context.Vehicles.AnyAsync(v => v.ManufacturerId == id))
            {
                return BadRequest("Cannot delete manufacturer as it is being used by vehicles");
            }

            _context.Manufacturers.Remove(manufacturer);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ManufacturerExists(int id)
        {
            return _context.Manufacturers.Any(e => e.Id == id);
        }
    }
}
