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
    public class ServicesTypeSetUpsController : ControllerBase
    {
        private readonly GarageClientContext _context;

        public ServicesTypeSetUpsController(GarageClientContext context)
        {
            _context = context;
        }

        // GET: api/ServicesTypeSetUps
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ServicesTypeSetUp>>> GetServicesTypeSetUps()
        {
            return await _context.ServicesTypeSetUps
                .Include(s => s.MeassureUnit)
                .Include(s => s.ServiceTypes)
                .OrderBy(s => s.ServiceTypes.Description)
                .ThenBy(s => s.ServiceTypesValue)
                .ToListAsync();
        }

        // GET: api/ServicesTypeSetUps/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ServicesTypeSetUp>> GetServicesTypeSetUp(int id)
        {
            var servicesTypeSetUp = await _context.ServicesTypeSetUps
                .Include(s => s.MeassureUnit)
                .Include(s => s.ServiceTypes)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (servicesTypeSetUp == null)
            {
                return NotFound();
            }

            return servicesTypeSetUp;
        }

        // GET: api/ServicesTypeSetUps/service/5
        [HttpGet("service/{serviceTypeId}")]
        public async Task<ActionResult<IEnumerable<ServicesTypeSetUp>>> GetSetupsByServiceType(int serviceTypeId)
        {
            return await _context.ServicesTypeSetUps
                .Where(s => s.ServiceTypesid == serviceTypeId)
                .Include(s => s.MeassureUnit)
                .OrderBy(s => s.ServiceTypesValue)
                .ToListAsync();
        }

        // GET: api/ServicesTypeSetUps/Vehicle/5
        [HttpGet("Vehicle/{Vehicleid}")]
        public async Task<ActionResult<IEnumerable<ServicesTypeSetUp>>> GetSetupsByVehicle(int Vehicleid)
        {
            return await _context.ServicesTypeSetUps
                .Where(s => s.Vehicleid == Vehicleid)
                //.Include(s => s.MeassureUnit)
                //.Include(s => s.Vehicle)
                .OrderBy(s => s.ServiceTypesValue)
                .ToListAsync();
        }

        // GET: api/ServicesTypeSetUps/unit/3
        [HttpGet("unit/{measureUnitId}")]
        public async Task<ActionResult<IEnumerable<ServicesTypeSetUp>>> GetSetupsByMeasureUnit(int measureUnitId)
        {
            return await _context.ServicesTypeSetUps
                .Where(s => s.MeassureUnitid == measureUnitId)
                .Include(s => s.ServiceTypes)
                .OrderBy(s => s.ServiceTypes.Description)
                .ToListAsync();
        }

        // GET: api/ServicesTypeSetUps/lookup?serviceTypeId=5&value=100
        [HttpGet("lookup")]
        public async Task<ActionResult<ServicesTypeSetUp>> LookupSetup([FromQuery] int serviceTypeId, [FromQuery] int value)
        {
            var setup = await _context.ServicesTypeSetUps
                .Where(s => s.ServiceTypesid == serviceTypeId && s.ServiceTypesValue == value)
                .Include(s => s.MeassureUnit)
                .Include(s => s.ServiceTypes)
                .FirstOrDefaultAsync();

            if (setup == null)
            {
                return NotFound();
            }

            return setup;
        }

        // POST: api/ServicesTypeSetUps
        [HttpPost]
        public async Task<ActionResult<ServicesTypeSetUp>> PostServicesTypeSetUp(ServicesTypeSetUp servicesTypeSetUp)
        {
            // Validate unique combination of ServiceTypesid and ServiceTypesValue
            if (await _context.ServicesTypeSetUps.AnyAsync(s =>
                s.ServiceTypesid == servicesTypeSetUp.ServiceTypesid &&
                s.Vehicleid == servicesTypeSetUp.Vehicleid &&
                s.ServiceTypesValue == servicesTypeSetUp.ServiceTypesValue))
            {
                return Conflict("A setup with this service type and value already exists");
            }

            _context.ServicesTypeSetUps.Add(servicesTypeSetUp);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetServicesTypeSetUp", new { id = servicesTypeSetUp.Id }, servicesTypeSetUp);
        }

        // PUT: api/ServicesTypeSetUps/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutServicesTypeSetUp(int id, ServicesTypeSetUp servicesTypeSetUp)
        {
            if (id != servicesTypeSetUp.Id)
            {
                return BadRequest();
            }

            // Validate unique combination (excluding current record)
            if (await _context.ServicesTypeSetUps.AnyAsync(s =>
                s.Id != id &&
                s.ServiceTypesid == servicesTypeSetUp.ServiceTypesid &&
                s.Vehicleid == servicesTypeSetUp.Vehicleid &&
                s.ServiceTypesValue == servicesTypeSetUp.ServiceTypesValue))
            {
                return Conflict("A setup with this service type and value already exists");
            }

            _context.Entry(servicesTypeSetUp).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ServicesTypeSetUpExists(id))
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

        // PATCH: api/ServicesTypeSetUps/5/value
        [HttpPatch("{id}/value")]
        public async Task<IActionResult> UpdateServiceTypeValue(int id, [FromBody] int newValue)
        {
            var setup = await _context.ServicesTypeSetUps.FindAsync(id);
            if (setup == null)
            {
                return NotFound();
            }

            // Validate unique combination
            if (await _context.ServicesTypeSetUps.AnyAsync(s =>
                s.Id != id &&
                s.ServiceTypesid == setup.ServiceTypesid &&
                s.ServiceTypesValue == newValue))
            {
                return Conflict("A setup with this service type and value already exists");
            }

            setup.ServiceTypesValue = newValue;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/ServicesTypeSetUps/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteServicesTypeSetUp(int id)
        {
            var servicesTypeSetUp = await _context.ServicesTypeSetUps.FindAsync(id);
            if (servicesTypeSetUp == null)
            {
                return NotFound();
            }

            _context.ServicesTypeSetUps.Remove(servicesTypeSetUp);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ServicesTypeSetUpExists(int id)
        {
            return _context.ServicesTypeSetUps.Any(e => e.Id == id);
        }
    }
}
