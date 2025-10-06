using GarageClientAPI.Data;
using GarageClientAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace GarageClientAPI.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class VehiclesController : ControllerBase
    {
        private readonly GarageClientContext _context;

        public VehiclesController(GarageClientContext context)
        {
            _context = context;
        }

        // GET: api/Vehicles
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Vehicle>>> GetVehicles()
        {
            return await _context.Vehicles
                .Include(v => v.Client)
                .Include(v => v.FuelType)
                .Include(v => v.Manufacturer)
                .Include(v => v.MeassureUnit)
                .Include(v => v.VehicleType)
                .OrderBy(v => v.LiscencePlate)
                .ToListAsync();
        }

        // GET: api/Vehicles/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Vehicle>> GetVehicle(int id)
        {
            var vehicle = await _context.Vehicles
                .Include(v => v.Client)
                .Include(v => v.FuelType)
                .Include(v => v.Manufacturer)
                .Include(v => v.MeassureUnit)
                .Include(v => v.VehicleType)
                .Include(v => v.VehicleAppointments)
                .Include(v => v.VehicleChecks)
                .Include(v => v.VehiclesRefuels)
                .Include(v => v.VehiclesServices)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (vehicle == null)
            {
                return NotFound();
            }

            return vehicle;
        }

        // GET: api/Vehicles/client/5
        [HttpGet("client/{clientId}")]
        public async Task<ActionResult<IEnumerable<Vehicle>>> GetVehiclesByClient(int clientId)
        {
            return await _context.Vehicles
                .Where(v => v.ClientId == clientId)
                .Include(v => v.FuelType)
                .Include(v => v.Manufacturer)
                .Include(v => v.VehicleType)
                .OrderBy(v => v.LiscencePlate)
                .ToListAsync();
        }

        // GET: api/Vehicles/license/{licensePlate}
        [HttpGet("license/{licensePlate}")]
        public async Task<ActionResult<Vehicle>> GetVehicleByLicensePlate(string licensePlate)
        {
            var vehicle = await _context.Vehicles
                .Include(v => v.Client)
                .Include(v => v.FuelType)
                .Include(v => v.Manufacturer)
                .Include(v => v.VehicleType)
                .FirstOrDefaultAsync(v => v.LiscencePlate == licensePlate);

            if (vehicle == null)
            {
                return NotFound();
            }

            return vehicle;
        }

        // GET: api/Vehicles/active
        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<Vehicle>>> GetActiveVehicles()
        {
            return await _context.Vehicles
                .Where(v => v.Active)
                .Include(v => v.Client)
                .Include(v => v.Manufacturer)
                .Include(v => v.VehicleType)
                .OrderBy(v => v.LiscencePlate)
                .ToListAsync();
        }

        // GET: api/Vehicles/manufacturer/5
        [HttpGet("manufacturer/{manufacturerId}")]
        public async Task<ActionResult<IEnumerable<Vehicle>>> GetVehiclesByManufacturer(int manufacturerId)
        {
            return await _context.Vehicles
                .Where(v => v.ManufacturerId == manufacturerId)
                .Include(v => v.Client)
                .Include(v => v.VehicleType)
                .OrderBy(v => v.LiscencePlate)
                .ToListAsync();
        }

        // POST: api/Vehicles
        [HttpPost]
        public async Task<ActionResult<Vehicle>> PostVehicle(Vehicle vehicle)
        {
            // Validate required relationships
            if (!await ValidateVehicleRelationships(vehicle))
            {
                return BadRequest("Invalid related entity ID(s)");
            }

            // Validate license plate is unique
            if (await _context.Vehicles.AnyAsync(v => v.LiscencePlate == vehicle.LiscencePlate))
            {
                return Conflict("A vehicle with this license plate already exists");
            }

            // Validate chassis number is unique
            if (await _context.Vehicles.AnyAsync(v => v.ChassisNumber == vehicle.ChassisNumber))
            {
                return Conflict("A vehicle with this chassis number already exists");
            }

            _context.Vehicles.Add(vehicle);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetVehicle", new { id = vehicle.Id }, vehicle);
        }

        // PUT: api/Vehicles/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutVehicle(int id, Vehicle vehicle)
        {
            if (id != vehicle.Id)
            {
                return BadRequest();
            }

            // Validate required relationships
            if (!await ValidateVehicleRelationships(vehicle))
            {
                return BadRequest("Invalid related entity ID(s)");
            }

            // Validate license plate is unique (excluding current vehicle)
            if (await _context.Vehicles.AnyAsync(v => v.LiscencePlate == vehicle.LiscencePlate && v.Id != id))
            {
                return Conflict("A vehicle with this license plate already exists");
            }

            // Validate chassis number is unique (excluding current vehicle)
            if (await _context.Vehicles.AnyAsync(v => v.ChassisNumber == vehicle.ChassisNumber && v.Id != id))
            {
                return Conflict("A vehicle with this chassis number already exists");
            }

            _context.Entry(vehicle).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VehicleExists(id))
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

        // PATCH: api/Vehicles/5/odometer
        [HttpPatch("{id}/odometer")]
        public async Task<IActionResult> UpdateOdometer(int id, [FromBody] int odometer)
        {
            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle == null)
            {
                return NotFound();
            }

            vehicle.Odometer = odometer;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PATCH: api/Vehicles/5/status
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] bool active)
        {
            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle == null)
            {
                return NotFound();
            }

            vehicle.Active = active;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Vehicles/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVehicle(int id)
        {
            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle == null)
            {
                return NotFound();
            }

            // Check if vehicle has related records
            if (await _context.VehicleAppointments.AnyAsync(va => va.Vehicleid == id) ||
                await _context.VehicleChecks.AnyAsync(vc => vc.Vehicleid == id) ||
                await _context.VehiclesRefuels.AnyAsync(vr => vr.Vehicleid == id) ||
                await _context.VehiclesServices.AnyAsync(vs => vs.Vehicleid == id))
            {
                return BadRequest("Cannot delete vehicle as it has related records");
            }

            _context.Vehicles.Remove(vehicle);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool VehicleExists(int id)
        {
            return _context.Vehicles.Any(e => e.Id == id);
        }

        private async Task<bool> ValidateVehicleRelationships(Vehicle vehicle)
        {
            return await _context.ClientProfiles.AnyAsync(c => c.Id == vehicle.ClientId) &&
                   await _context.FuelTypes.AnyAsync(f => f.Id == vehicle.FuelTypeId) &&
                   await _context.Manufacturers.AnyAsync(m => m.Id == vehicle.ManufacturerId) &&
                   await _context.MeassureUnits.AnyAsync(mu => mu.Id == vehicle.MeassureUnitId) &&
                   await _context.VehicleTypes.AnyAsync(vt => vt.Id == vehicle.VehicleTypeId);
        }

        // GET: api/Vehicles/count
        [HttpGet("count")]
        public async Task<ActionResult<int>> GetVehicleCount()
        {
            return await _context.Vehicles.CountAsync();
        }

        // GET: api/Vehicles/search?query=ABC123
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<Vehicle>>> SearchVehicles([FromQuery] string query)
        {
            return await _context.Vehicles
                .Where(v => v.LiscencePlate.Contains(query) ||
                           v.ChassisNumber.Contains(query) ||
                           v.VehicleName.Contains(query) ||
                           v.Model.Contains(query))
                .Include(v => v.Client)
                .Include(v => v.Manufacturer)
                .Include(v => v.VehicleType)
                .OrderBy(v => v.LiscencePlate)
                .ToListAsync();
        }

        // GET: api/Vehicles/5/history
        [HttpGet("GetVehicleHistory/{id}")]
        public async Task<ActionResult<object>> GetVehicleHistory(int id)
        {
            var vehicles = await _context.Vehicles.FindAsync(id);
            if (vehicles == null)
            {
                return NotFound();
            }

            return new
            {
                Appointments = await _context.VehicleAppointments
                    .Include(va => va.Vehicle)
                    .Where(va => va.Vehicleid == id)
                    .OrderByDescending(va => va.AppointmentDate)
                    .ToListAsync(),
                Checks = await _context.VehicleChecks
                    .Where(vc => vc.Vehicleid == id)
                    .OrderByDescending(vs => vs.Id)
                    .ToListAsync(),
                Refuels = await _context.VehiclesRefuels
                    .Include(vr => vr.Vehicle)
                    .Where(vr => vr.Vehicleid == id)
                    .OrderByDescending(vs => vs.RefuleDate)
                    .ToListAsync(),
                Services = await _context.VehiclesServices
                    .Include(vc => vc.Vehicle)
                    .Include(vc => vc.VehiclesServiceTypes)
                      .ThenInclude(vst => vst.ServiceType )
                      
                    .Include(vc => vc.Garage)
                    .Where(vs => vs.Vehicleid == id)
                    .OrderByDescending(vs => vs.ServiceDate)
                    .ToListAsync()
            };
        }

        [HttpGet("GetVehicleServicesHistory/{VehicleID}")]
        public async Task<ActionResult<bool>> GetVehicleServicesHistory(int VehicleID,int CurrentOdometer)
        {
            var vehicles = await _context.Vehicles.FindAsync(VehicleID);
            if (vehicles == null)
            {
                return NotFound();
            }

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC CheckRemainingServices @VehicleID, @CurrentOdometer",
                new SqlParameter("@VehicleID", VehicleID),
                new SqlParameter("@CurrentOdometer", CurrentOdometer)
            );
            return true;
        }
    }
}
