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
    public class CountriesController : ControllerBase
    {
        private readonly GarageClientContext _context;

        public CountriesController(GarageClientContext context)
        {
            _context = context;
        }

        // GET: api/Countries
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Country>>> GetCountries()
        {
            return await _context.Countries
                .OrderBy(c => c.CountryName)
                .ToListAsync();
        }

        // GET: api/Countries/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Country>> GetCountry(int id)
        {
            var country = await _context.Countries.FindAsync(id);

            if (country == null)
            {
                return NotFound();
            }

            return country;
        }

        // GET: api/Countries/5/clients
        [HttpGet("{id}/clients")]
        public async Task<ActionResult<IEnumerable<ClientProfile>>> GetClientsByCountry(int id)
        {
            return await _context.ClientProfiles
                .Where(c => c.CountryId == id)
                .Include(c => c.User)
                .OrderBy(c => c.LastName)
                .ThenBy(c => c.FirstName)
                .ToListAsync();
        }

        // GET: api/Countries/5/garages
        [HttpGet("{id}/garages")]
        public async Task<ActionResult<IEnumerable<GarageProfile>>> GetGaragesByCountry(int id)
        {
            return await _context.GarageProfiles
                .Where(g => g.CountryId == id)
                .OrderBy(g => g.GarageName)
                .ToListAsync();
        }

        // GET: api/Countries/search?name={name}
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<Country>>> SearchCountries([FromQuery] string name)
        {
            return await _context.Countries
                .Where(c => c.CountryName.Contains(name))
                .OrderBy(c => c.CountryName)
                .ToListAsync();
        }

        // POST: api/Countries
        [HttpPost]
        public async Task<ActionResult<Country>> PostCountry(Country country)
        {
            // Validate country name is unique
            if (await _context.Countries.AnyAsync(c => c.CountryName == country.CountryName))
            {
                return Conflict("A country with this name already exists");
            }

            _context.Countries.Add(country);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCountry", new { id = country.Id }, country);
        }

        // PUT: api/Countries/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCountry(int id, Country country)
        {
            if (id != country.Id)
            {
                return BadRequest();
            }

            // Validate country name is unique (excluding current country)
            if (await _context.Countries.AnyAsync(c => c.CountryName == country.CountryName && c.Id != id))
            {
                return Conflict("A country with this name already exists");
            }

            _context.Entry(country).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CountryExists(id))
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

        // PATCH: api/Countries/5/flag
        [HttpPatch("{id}/flag")]
        public async Task<IActionResult> UpdateCountryFlag(int id, [FromBody] byte[] flag)
        {
            var country = await _context.Countries.FindAsync(id);
            if (country == null)
            {
                return NotFound();
            }

            country.CountryFlag = flag;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Countries/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCountry(int id)
        {
            var country = await _context.Countries.FindAsync(id);
            if (country == null)
            {
                return NotFound();
            }

            // Check if country is in use
            if (await _context.ClientProfiles.AnyAsync(c => c.CountryId == id) ||
                await _context.GarageProfiles.AnyAsync(g => g.CountryId == id))
            {
                return BadRequest("Cannot delete country as it is being used by clients or garages");
            }

            _context.Countries.Remove(country);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CountryExists(int id)
        {
            return _context.Countries.Any(e => e.Id == id);
        }
    }
}
