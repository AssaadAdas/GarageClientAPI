using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using GarageClientAPI.Data;
using GarageClientAPI.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GarageClientAPI.Controllers
{


    [Route("api/[controller]")]
    [ApiController]
    public class VehicleChecksController : ControllerBase
    {
        private readonly GarageClientContext _context;

        public VehicleChecksController(GarageClientContext context)
        {
            _context = context;
        }

        // GET: api/VehicleChecks
        [HttpGet]
        public async Task<ActionResult<IEnumerable<VehicleCheck>>> GetVehicleChecks()
        {
            return await _context.VehicleChecks
                .Include(vc => vc.Vehicle)
                    .ThenInclude(v => v.Client)
                .OrderByDescending(vc => vc.Id)
                .ToListAsync();
        }

        // GET: api/VehicleChecks/5
        [HttpGet("{id}")]
        public async Task<ActionResult<VehicleCheck>> GetVehicleCheck(int id)
        {
            var vehicleCheck = await _context.VehicleChecks
                .Include(vc => vc.Vehicle)
                    .ThenInclude(v => v.Client)
                .FirstOrDefaultAsync(vc => vc.Id == id);

            if (vehicleCheck == null)
            {
                return NotFound();
            }

            return vehicleCheck;
        }

        // GET: api/VehicleChecks/vehicle/5
        [HttpGet("vehicle/{vehicleId}")]
        public async Task<ActionResult<IEnumerable<VehicleCheck>>> GetChecksByVehicle(int vehicleId)
        {
            return await _context.VehicleChecks
                .Where(vc => vc.Vehicleid == vehicleId)
                .Include(vc => vc.Vehicle)
                .OrderByDescending(vc => vc.Id)
                .ToListAsync();
        }

        // GET: api/VehicleChecks/status/passed
        [HttpGet("status/{status}")]
        public async Task<ActionResult<IEnumerable<VehicleCheck>>> GetChecksByStatus(string status)
        {
            return await _context.VehicleChecks
                .Where(vc => vc.CheckStatus == status)
                .Include(vc => vc.Vehicle)
                    .ThenInclude(v => v.Client)
                .OrderByDescending(vc => vc.Id)
                .ToListAsync();
        }

        // POST: api/VehicleChecks
        [HttpPost]
        public async Task<ActionResult<VehicleCheck>> PostVehicleCheck(VehicleCheck vehicleCheck)
        {
            // Validate vehicle exists if provided
            if (vehicleCheck.Vehicleid.HasValue &&
                !await _context.Vehicles.AnyAsync(v => v.Id == vehicleCheck.Vehicleid.Value))
            {
                return BadRequest("Invalid Vehicle ID");
            }

            // Set default status if not provided
            if (string.IsNullOrEmpty(vehicleCheck.CheckStatus))
            {
                vehicleCheck.CheckStatus = "pnd";
            }

            _context.VehicleChecks.Add(vehicleCheck);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetVehicleCheck", new { id = vehicleCheck.Id }, vehicleCheck);
        }

        // PUT: api/VehicleChecks/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutVehicleCheck(int id, VehicleCheck vehicleCheck)
        {
            if (id != vehicleCheck.Id)
            {
                return BadRequest();
            }

            // Validate vehicle exists if provided
            if (vehicleCheck.Vehicleid.HasValue &&
                !await _context.Vehicles.AnyAsync(v => v.Id == vehicleCheck.Vehicleid.Value))
            {
                return BadRequest("Invalid Vehicle ID");
            }

            _context.Entry(vehicleCheck).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VehicleCheckExists(id))
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

        // PATCH: api/VehicleChecks/5/status
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateCheckStatus(int id, [FromBody] string status)
        {
            var check = await _context.VehicleChecks.FindAsync(id);
            if (check == null)
            {
                return NotFound();
            }

            check.CheckStatus = status;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PATCH: api/VehicleChecks/5/assign
        [HttpPatch("{id}/assign")]
        public async Task<IActionResult> AssignToVehicle(int id, [FromBody] int vehicleId)
        {
            var check = await _context.VehicleChecks.FindAsync(id);
            if (check == null)
            {
                return NotFound();
            }

            if (!await _context.Vehicles.AnyAsync(v => v.Id == vehicleId))
            {
                return BadRequest("Invalid Vehicle ID");
            }

            check.Vehicleid = vehicleId;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/VehicleChecks/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVehicleCheck(int id)
        {
            var vehicleCheck = await _context.VehicleChecks.FindAsync(id);
            if (vehicleCheck == null)
            {
                return NotFound();
            }

            _context.VehicleChecks.Remove(vehicleCheck);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/VehicleChecks/recent
        [HttpGet("recent")]
        public async Task<ActionResult<IEnumerable<VehicleCheck>>> GetRecentChecks()
        {
            return await _context.VehicleChecks
                .Include(vc => vc.Vehicle)
                    .ThenInclude(v => v.Client)
                .OrderByDescending(vc => vc.Id)
                .Take(10)
                .ToListAsync();
        }

        // GET: api/VehicleChecks/unassigned
        [HttpGet("unassigned")]
        public async Task<ActionResult<IEnumerable<VehicleCheck>>> GetUnassignedChecks()
        {
            return await _context.VehicleChecks
                .Where(vc => vc.Vehicleid == null)
                .OrderByDescending(vc => vc.Id)
                .ToListAsync();
        }

        // GET: api/VehicleChecks/summary
        [HttpGet("summary")]
        public async Task<ActionResult<object>> GetChecksSummary()
        {
            return new
            {
                TotalChecks = await _context.VehicleChecks.CountAsync(),
                PassedChecks = await _context.VehicleChecks.CountAsync(vc => vc.CheckStatus == "Passed"),
                FailedChecks = await _context.VehicleChecks.CountAsync(vc => vc.CheckStatus == "Failed"),
                PendingChecks = await _context.VehicleChecks.CountAsync(vc => vc.CheckStatus == "Pending")
            };
        }
        private bool VehicleCheckExists(int id)
        {
            return _context.VehicleChecks.Any(e => e.Id == id);
        }
    }
}
