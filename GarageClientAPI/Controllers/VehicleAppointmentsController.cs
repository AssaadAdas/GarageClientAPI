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
    public class VehicleAppointmentsController : ControllerBase
    {
        private readonly GarageClientContext _context;

        public VehicleAppointmentsController(GarageClientContext context)
        {
            _context = context;
        }

        // GET: api/VehicleAppointments
        [HttpGet]
        public async Task<ActionResult<IEnumerable<VehicleAppointment>>> GetVehicleAppointments()
        {
            return await _context.VehicleAppointments
                .Include(va => va.Vehicle)
                    .ThenInclude(v => v.Client)
                .OrderByDescending(va => va.AppointmentDate)
                .ToListAsync();
        }

        // GET: api/VehicleAppointments/5
        [HttpGet("{id}")]
        public async Task<ActionResult<VehicleAppointment>> GetVehicleAppointment(int id)
        {
            var vehicleAppointment = await _context.VehicleAppointments
                .Include(va => va.Garage)
                .Include(va => va.Vehicle)
                    .ThenInclude(v => v.Client)
                .FirstOrDefaultAsync(va => va.Id == id);

            if (vehicleAppointment == null)
            {
                return NotFound();
            }

            return vehicleAppointment;
        }

        // GET: api/VehicleAppointments/vehicle/5
        [HttpGet("vehicle/{vehicleId}")]
        public async Task<ActionResult<IEnumerable<VehicleAppointment>>> GetAppointmentsByVehicle(int vehicleId)
        {
            return await _context.VehicleAppointments
                .Where(va => va.Vehicleid == vehicleId)
                .Include(va => va.Garage)
                .Include(va => va.Vehicle)
                  .ThenInclude(v => v.Client)
                .OrderByDescending(va => va.AppointmentDate)
                .ToListAsync();
        }

        // GET: api/VehicleAppointments/Garage/5
        [HttpGet("Garage/{garageId}")]
        public async Task<ActionResult<IEnumerable<VehicleAppointment>>> GetAppointmentsByGarage(int garageId)
        {
            return await _context.VehicleAppointments
                .Where(va => va.Garageid == garageId)
                .Include(va => va.Garage)
                .Include(va => va.Vehicle)
                  .ThenInclude(v => v.Client)
                .OrderByDescending(va => va.AppointmentDate)
                .ToListAsync();
        }

        // GET: api/VehicleAppointments/upcoming
        [HttpGet("upcoming")]
        public async Task<ActionResult<IEnumerable<VehicleAppointment>>> GetUpcomingAppointments()
        {
            var today = DateTime.Today;
            return await _context.VehicleAppointments
                .Where(va => va.AppointmentDate >= today)
                .Include(va => va.Vehicle)
                    .ThenInclude(v => v.Client)
                .OrderBy(va => va.AppointmentDate)
                .Take(10)
                .ToListAsync();
        }

        // GET: api/VehicleAppointments/date/2023-05-15
        [HttpGet("date/{date}")]
        public async Task<ActionResult<IEnumerable<VehicleAppointment>>> GetAppointmentsByDate(DateTime date)
        {
            return await _context.VehicleAppointments
                .Where(va => va.AppointmentDate.Date == date.Date)
                .Include(va => va.Vehicle)
                    .ThenInclude(v => v.Client)
                .OrderBy(va => va.AppointmentDate)
                .ToListAsync();
        }

        // POST: api/VehicleAppointments
        [HttpPost]
        public async Task<ActionResult<VehicleAppointment>> PostVehicleAppointment(VehicleAppointment vehicleAppointment)
        {
            // Validate vehicle exists
            if (!await _context.Vehicles.AnyAsync(v => v.Id == vehicleAppointment.Vehicleid))
            {
                return BadRequest("Invalid Vehicle ID");
            }

            _context.VehicleAppointments.Add(vehicleAppointment);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetVehicleAppointment", new { id = vehicleAppointment.Id }, vehicleAppointment);
        }

        // PUT: api/VehicleAppointments/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutVehicleAppointment(int id, VehicleAppointment vehicleAppointment)
        {
            if (id != vehicleAppointment.Id)
            {
                return BadRequest();
            }

            // Validate vehicle exists
            if (!await _context.Vehicles.AnyAsync(v => v.Id == vehicleAppointment.Vehicleid))
            {
                return BadRequest("Invalid Vehicle ID");
            }

            _context.Entry(vehicleAppointment).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VehicleAppointmentExists(id))
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

        // PATCH: api/VehicleAppointments/5/reschedule
        [HttpPatch("{id}/reschedule")]
        public async Task<IActionResult> RescheduleAppointment(int id, [FromBody] DateTime newDate)
        {
            var appointment = await _context.VehicleAppointments.FindAsync(id);
            if (appointment == null)
            {
                return NotFound();
            }

            appointment.AppointmentDate = newDate;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PATCH: api/VehicleAppointments/5/note
        [HttpPatch("{id}/note")]
        public async Task<IActionResult> UpdateAppointmentNote(int id, [FromBody] string note)
        {
            var appointment = await _context.VehicleAppointments.FindAsync(id);
            if (appointment == null)
            {
                return NotFound();
            }

            appointment.Note = note;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/VehicleAppointments/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVehicleAppointment(int id)
        {
            var vehicleAppointment = await _context.VehicleAppointments.FindAsync(id);
            if (vehicleAppointment == null)
            {
                return NotFound();
            }

            _context.VehicleAppointments.Remove(vehicleAppointment);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/VehicleAppointments/client/5
        [HttpGet("client/{clientId}")]
        public async Task<ActionResult<IEnumerable<VehicleAppointment>>> GetAppointmentsByClient(int clientId)
        {
            return await _context.VehicleAppointments
                .Include(va => va.Vehicle)
                .Where(va => va.Vehicle.ClientId == clientId)
                .OrderByDescending(va => va.AppointmentDate)
                .ToListAsync();
        }

        // GET: api/VehicleAppointments/range?start=2023-05-01&end=2023-05-31
        [HttpGet("range")]
        public async Task<ActionResult<IEnumerable<VehicleAppointment>>> GetAppointmentsInRange(
            [FromQuery] DateTime start,
            [FromQuery] DateTime end)
        {
            return await _context.VehicleAppointments
                .Where(va => va.AppointmentDate >= start && va.AppointmentDate <= end)
                .Include(va => va.Vehicle)
                    .ThenInclude(v => v.Client)
                .OrderBy(va => va.AppointmentDate)
                .ToListAsync();
        }

        // GET: api/VehicleAppointments/count
        [HttpGet("count")]
        public async Task<ActionResult<int>> GetAppointmentCount()
        {
            return await _context.VehicleAppointments.CountAsync();
        }
        private bool VehicleAppointmentExists(int id)
        {
            return _context.VehicleAppointments.Any(e => e.Id == id);
        }
    }
}
