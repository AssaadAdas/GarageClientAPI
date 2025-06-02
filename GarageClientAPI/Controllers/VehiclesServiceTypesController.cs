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
    public class VehiclesServiceTypesController : ControllerBase
    {
        private readonly GarageClientContext _context;

        public VehiclesServiceTypesController(GarageClientContext context)
        {
            _context = context;
        }

        // GET: api/VehiclesServiceTypes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<VehiclesServiceType>>> GetVehiclesServiceTypes()
        {
            return await _context.VehiclesServiceTypes
                .Include(vst => vst.VehicleService)
                .Include(vst => vst.Curr)
                .Include(vst => vst.ServiceType)
                .ToListAsync();
        }

        // GET: api/VehiclesServiceTypes/service/5 (by VehicleServiceId)
        [HttpGet("service/{vehicleServiceId}")]
        public async Task<ActionResult<IEnumerable<VehiclesServiceType>>> GetVehiclesServiceTypesByService(int vehicleServiceId)
        {
            return await _context.VehiclesServiceTypes
                .Where(vst => vst.VehicleServiceId == vehicleServiceId)
                .Include(vst => vst.VehicleService)
                .Include(vst => vst.Curr)
                .Include(vst => vst.ServiceType)
                .ToListAsync();
        }

        // GET: api/VehiclesServiceTypes/5/10 (composite key)
        [HttpGet("{vehicleServiceId}/{serviceTypeId}")]
        public async Task<ActionResult<VehiclesServiceType>> GetVehiclesServiceType(int vehicleServiceId, int serviceTypeId)
        {
            var vehiclesServiceType = await _context.VehiclesServiceTypes
                .Include(vst => vst.VehicleService)
                .Include(vst => vst.Curr)
                .Include(vst => vst.ServiceType)
                .FirstOrDefaultAsync(vst => vst.VehicleServiceId == vehicleServiceId && vst.ServiceTypeId == serviceTypeId);

            if (vehiclesServiceType == null)
            {
                return NotFound();
            }

            return vehiclesServiceType;
        }

        // POST: api/VehiclesServiceTypes
        [HttpPost]
        public async Task<ActionResult<VehiclesServiceType>> PostVehiclesServiceType(VehiclesServiceType vehiclesServiceType)
        {
            _context.VehiclesServiceTypes.Add(vehiclesServiceType);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (VehiclesServiceTypeExists(vehiclesServiceType.VehicleServiceId, vehiclesServiceType.ServiceTypeId))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetVehiclesServiceType",
                new { vehicleServiceId = vehiclesServiceType.VehicleServiceId, serviceTypeId = vehiclesServiceType.ServiceTypeId },
                vehiclesServiceType);
        }

        // PUT: api/VehiclesServiceTypes/5/10
        [HttpPut("{vehicleServiceId}/{serviceTypeId}")]
        public async Task<IActionResult> PutVehiclesServiceType(int vehicleServiceId, int serviceTypeId, VehiclesServiceType vehiclesServiceType)
        {
            if (vehicleServiceId != vehiclesServiceType.VehicleServiceId || serviceTypeId != vehiclesServiceType.ServiceTypeId)
            {
                return BadRequest();
            }

            _context.Entry(vehiclesServiceType).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VehiclesServiceTypeExists(vehicleServiceId, serviceTypeId))
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

        // DELETE: api/VehiclesServiceTypes/5/10
        [HttpDelete("{vehicleServiceId}/{serviceTypeId}")]
        public async Task<IActionResult> DeleteVehiclesServiceType(int vehicleServiceId, int serviceTypeId)
        {
            var vehiclesServiceType = await _context.VehiclesServiceTypes
                .FirstOrDefaultAsync(vst => vst.VehicleServiceId == vehicleServiceId && vst.ServiceTypeId == serviceTypeId);

            if (vehiclesServiceType == null)
            {
                return NotFound();
            }

            _context.VehiclesServiceTypes.Remove(vehiclesServiceType);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool VehiclesServiceTypeExists(int vehicleServiceId, int serviceTypeId)
        {
            return _context.VehiclesServiceTypes.Any(e => e.VehicleServiceId == vehicleServiceId && e.ServiceTypeId == serviceTypeId);
        }
    }
}
