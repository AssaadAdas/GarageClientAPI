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
    public class VehiclesServicesController : ControllerBase
    {
        private readonly GarageClientContext _context;

        public VehiclesServicesController(GarageClientContext context)
        {
            _context = context;
        }

        // GET: api/VehiclesServices
        [HttpGet]
        public async Task<ActionResult<IEnumerable<VehiclesService>>> GetVehiclesServices()
        {
            return await _context.VehiclesServices
                //.Include(vs => vs.Garage)
                //.Include(vs => vs.Vehicle)
                //.Include(vs => vs.VehiclesServiceTypes)
                .ToListAsync();
        }

        // GET: api/VehiclesServices/5
        [HttpGet("{id}")]
        public async Task<ActionResult<VehiclesService>> GetVehiclesService(int id)
        {
            var vehiclesService = await _context.VehiclesServices
                //.Include(vs => vs.Garage)
                //.Include(vs => vs.Vehicle)
                //.Include(vs => vs.VehiclesServiceTypes)
                .FirstOrDefaultAsync(vs => vs.Id == id);

            if (vehiclesService == null)
            {
                return NotFound();
            }

            return vehiclesService;
        }

        // PUT: api/VehiclesServices/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutVehiclesService(int id, VehiclesService vehiclesService)
        {
            if (id != vehiclesService.Id)
            {
                return BadRequest();
            }

            _context.Entry(vehiclesService).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VehiclesServiceExists(id))
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

        // POST: api/VehiclesServices
        [HttpPost]
        public async Task<ActionResult<VehiclesService>> PostVehiclesService(VehiclesService vehiclesService)
        {
            // Validate the model
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if vehicle exists
            var vehicleExists = await _context.Vehicles.AnyAsync(v => v.Id == vehiclesService.Vehicleid);
            if (!vehicleExists)
            {
                return NotFound($"Vehicle with ID {vehiclesService.Vehicleid} not found.");
            }

            // Check if garage exists
            var garageExists = await _context.GarageProfiles.AnyAsync(g => g.Id == vehiclesService.Garageid);
            if (!garageExists)
            {
                return NotFound($"Garage with ID {vehiclesService.Garageid} not found.");
            }

            // Set default values if needed
            

            _context.VehiclesServices.Add(vehiclesService);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetVehiclesService", new { id = vehiclesService.Id }, vehiclesService);
        }

        // DELETE: api/VehiclesServices/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVehiclesService(int id)
        {
            var vehiclesService = await _context.VehiclesServices.FindAsync(id);
            if (vehiclesService == null)
            {
                return NotFound();
            }

            _context.VehiclesServices.Remove(vehiclesService);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        // GET: api/VehiclesServices/vehicle/{vehicleId}/last
        [HttpGet("vehicle/{vehicleId}/last")]
        public async Task<ActionResult<VehiclesService>> GetLastVehiclesServiceByVehicleId(int vehicleId)
        {
            var lastService = await _context.VehiclesServices
                // include the collection then include each related navigation separately
                .Include(vs => vs.VehiclesServiceTypes)
                    .ThenInclude(vst => vst.ServiceType)
                .Include(vs => vs.VehiclesServiceTypes)
                    .ThenInclude(vst => vst.Curr)
                .Include(vs => vs.Garage)
                .Include(vs => vs.Vehicle)
                .Where(vs => vs.Vehicleid == vehicleId)
                .OrderByDescending(vs => vs.ServiceDate)
                .FirstOrDefaultAsync();

            if (lastService == null)
            {
                return NotFound();
            }

            return lastService;
        }
        private bool VehiclesServiceExists(int id)
        {
            return _context.VehiclesServices.Any(e => e.Id == id);
        }
    }
}
