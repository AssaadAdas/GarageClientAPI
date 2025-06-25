using Microsoft.AspNetCore.Http;
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
    public class VehiclesRefuelsController : ControllerBase
    {
        private readonly GarageClientContext _context;

        public VehiclesRefuelsController(GarageClientContext context)
        {
            _context = context;
        }

        // GET: api/VehiclesRefuels
        [HttpGet]
        public async Task<ActionResult<IEnumerable<VehiclesRefuel>>> GetVehiclesRefuels()
        {
            return await _context.VehiclesRefuels
                .Include(vr => vr.Vehicle)
                    .ThenInclude(v => v.Client)
                .OrderByDescending(vr => vr.Id)
                .ToListAsync();
        }

        // GET: api/VehiclesRefuels/5
        [HttpGet("{id}")]
        public async Task<ActionResult<VehiclesRefuel>> GetVehiclesRefuel(int id)
        {
            var vehiclesRefuel = await _context.VehiclesRefuels
                .Include(vr => vr.Vehicle)
                    .ThenInclude(v => v.Client)
                .FirstOrDefaultAsync(vr => vr.Id == id);

            if (vehiclesRefuel == null)
            {
                return NotFound();
            }

            return vehiclesRefuel;
        }

        // GET: api/VehiclesRefuels/vehicle/5
        [HttpGet("vehicle/{vehicleId}")]
        public async Task<ActionResult<IEnumerable<VehiclesRefuel>>> GetRefuelsByVehicle(int vehicleId)
        {
            return await _context.VehiclesRefuels
                .Where(vr => vr.Vehicleid == vehicleId)
                .Include(vr => vr.Vehicle)
                .OrderByDescending(vr => vr.Id)
                .ToListAsync();
        }

        // GET: api/VehiclesRefuels/recent
        [HttpGet("recent")]
        public async Task<ActionResult<IEnumerable<VehiclesRefuel>>> GetRecentRefuels()
        {
            return await _context.VehiclesRefuels
                .Include(vr => vr.Vehicle)
                    .ThenInclude(v => v.Client)
                .OrderByDescending(vr => vr.Id)
                .Take(10)
                .ToListAsync();
        }

        // GET: api/VehiclesRefuels/summary/5
        [HttpGet("summary/{vehicleId}")]
        public async Task<ActionResult<object>> GetRefuelSummary(int vehicleId)
        {
            var refuels = await _context.VehiclesRefuels
                .Where(vr => vr.Vehicleid == vehicleId)
                .OrderBy(vr => vr.Id)
                .ToListAsync();

            if (!refuels.Any())
            {
                return NotFound("No refuel records found for this vehicle");
            }

            return new
            {
                TotalRefuels = refuels.Count,
                TotalFuel = refuels.Sum(vr => vr.RefuleValue),
                TotalCost = refuels.Sum(vr => vr.RefuelCost),
                AverageFuelPerRefuel = refuels.Average(vr => vr.RefuleValue),
                AverageCostPerRefuel = refuels.Average(vr => vr.RefuelCost)
            };
        }

        // POST: api/VehiclesRefuels
        [HttpPost]
        public async Task<ActionResult<VehiclesRefuel>> PostVehiclesRefuel(VehiclesRefuel vehiclesRefuel)
        {
            // Validate vehicle exists
            if (!await _context.Vehicles.AnyAsync(v => v.Id == vehiclesRefuel.Vehicleid))
            {
                return BadRequest("Invalid Vehicle ID");
            }

            //// Set default odometer if not provided
            //if (!vehiclesRefuel.Ododmeter.HasValue)
            //{
            //    var vehicle = await _context.Vehicles.FindAsync(vehiclesRefuel.Vehicleid);
            //    vehiclesRefuel.Ododmeter = vehicle.Odometer;
            //}

            _context.VehiclesRefuels.Add(vehiclesRefuel);
            await _context.SaveChangesAsync();

            // Update vehicle odometer if current refuel odometer is higher
            var vehicleToUpdate = await _context.Vehicles.FindAsync(vehiclesRefuel.Vehicleid);
            //if (vehicleToUpdate != null &&
            //    vehiclesRefuel.Ododmeter.HasValue &&
            //    vehiclesRefuel.Ododmeter > vehicleToUpdate.Odometer)
            //{
            //    vehicleToUpdate.Odometer = vehiclesRefuel.Ododmeter.Value;
            //    await _context.SaveChangesAsync();
            //}

            return CreatedAtAction("GetVehiclesRefuel", new { id = vehiclesRefuel.Id }, vehiclesRefuel);
        }

        // PUT: api/VehiclesRefuels/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutVehiclesRefuel(int id, VehiclesRefuel vehiclesRefuel)
        {
            if (id != vehiclesRefuel.Id)
            {
                return BadRequest();
            }

            // Validate vehicle exists
            if (!await _context.Vehicles.AnyAsync(v => v.Id == vehiclesRefuel.Vehicleid))
            {
                return BadRequest("Invalid Vehicle ID");
            }

            _context.Entry(vehiclesRefuel).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VehiclesRefuelExists(id))
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

        // DELETE: api/VehiclesRefuels/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVehiclesRefuel(int id)
        {
            var vehiclesRefuel = await _context.VehiclesRefuels.FindAsync(id);
            if (vehiclesRefuel == null)
            {
                return NotFound();
            }

            _context.VehiclesRefuels.Remove(vehiclesRefuel);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool VehiclesRefuelExists(int id)
        {
            return _context.VehiclesRefuels.Any(e => e.Id == id);
        }
    }
}
