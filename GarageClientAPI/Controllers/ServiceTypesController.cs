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
    public class ServiceTypesController : ControllerBase
    {
        private readonly GarageClientContext _context;

        public ServiceTypesController(GarageClientContext context)
        {
            _context = context;
        }

        // GET: api/ServiceTypes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ServiceType>>> GetServiceTypes()
        {
            return await _context.ServiceTypes
                .OrderBy(s => s.Description)
                .ToListAsync();
        }

        // GET: api/ServiceTypes/selected
        [HttpGet("selected")]
        public async Task<ActionResult<IEnumerable<ServiceType>>> GetSelectedServiceTypes()
        {
            return await _context.ServiceTypes
                .Where(s => s.IsSelected == true)
                .OrderBy(s => s.Description)
                .ToListAsync();
        }

        // GET: api/ServiceTypes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ServiceType>> GetServiceType(int id)
        {
            var serviceType = await _context.ServiceTypes
                .Include(s => s.ServicesTypeSetUps)
                    .ThenInclude(st => st.MeassureUnit)
                .Include(s => s.VehiclesServiceTypes)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (serviceType == null)
            {
                return NotFound();
            }

            return serviceType;
        }

        // GET: api/ServiceTypes/search?term=oil
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<ServiceType>>> SearchServiceTypes([FromQuery] string term)
        {
            return await _context.ServiceTypes
                .Where(s => s.Description.Contains(term))
                .OrderBy(s => s.Description)
                .ToListAsync();
        }

        // GET: api/ServiceTypes/5/setups
        [HttpGet("{id}/setups")]
        public async Task<ActionResult<IEnumerable<ServicesTypeSetUp>>> GetServiceTypeSetups(int id)
        {
            return await _context.ServicesTypeSetUps
                .Where(s => s.ServiceTypesid == id)
                .Include(s => s.MeassureUnit)
                .OrderBy(s => s.ServiceTypesValue)
                .ToListAsync();
        }

        // GET: api/ServiceTypes/5/vehicles
        [HttpGet("{id}/vehicles")]
        public async Task<ActionResult<IEnumerable<VehiclesServiceType>>> GetServiceTypeVehicles(int id)
        {
            return await _context.VehiclesServiceTypes
                .Where(v => v.ServiceTypeId == id)
                .Include(v => v.VehicleService)
                .Include(v => v.Curr)
                //.OrderBy(v => v.Vehicle.LicensePlate)
                .ToListAsync();
        }

        // POST: api/ServiceTypes
        [HttpPost]
        public async Task<ActionResult<ServiceType>> PostServiceType(ServiceType serviceType)
        {
            // Validate description is unique
            if (await _context.ServiceTypes.AnyAsync(s => s.Description == serviceType.Description))
            {
                return Conflict("A service type with this description already exists");
            }

            // Set default for IsSelected if not provided
            serviceType.IsSelected ??= false;

            _context.ServiceTypes.Add(serviceType);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetServiceType", new { id = serviceType.Id }, serviceType);
        }

        // PUT: api/ServiceTypes/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutServiceType(int id, ServiceType serviceType)
        {
            if (id != serviceType.Id)
            {
                return BadRequest();
            }

            // Validate description is unique (excluding current service type)
            if (await _context.ServiceTypes.AnyAsync(s => s.Description == serviceType.Description && s.Id != id))
            {
                return Conflict("A service type with this description already exists");
            }

            _context.Entry(serviceType).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ServiceTypeExists(id))
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

        // PATCH: api/ServiceTypes/5/select
        [HttpPatch("{id}/select")]
        public async Task<IActionResult> SelectServiceType(int id, [FromBody] bool isSelected)
        {
            var serviceType = await _context.ServiceTypes.FindAsync(id);
            if (serviceType == null)
            {
                return NotFound();
            }

            serviceType.IsSelected = isSelected;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/ServiceTypes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteServiceType(int id)
        {
            var serviceType = await _context.ServiceTypes.FindAsync(id);
            if (serviceType == null)
            {
                return NotFound();
            }

            // Check if service type is in use
            if (await _context.ServicesTypeSetUps.AnyAsync(s => s.ServiceTypesid == id) ||
                await _context.VehiclesServiceTypes.AnyAsync(v => v.ServiceTypeId == id))
            {
                return BadRequest("Cannot delete service type as it is being used in setups or vehicle services");
            }

            _context.ServiceTypes.Remove(serviceType);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ServiceTypeExists(int id)
        {
            return _context.ServiceTypes.Any(e => e.Id == id);
        }
    }
}
