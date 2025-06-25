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
    public class ClientRemindersController : ControllerBase
    {
        private readonly GarageClientContext _context;

        public ClientRemindersController(GarageClientContext context)
        {
            _context = context;
        }

        // GET: api/ClientReminders
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ClientReminder>>> GetClientReminders()
        {
            return await _context.ClientReminders
                .Include(r => r.Client)
                .ToListAsync();
        }

        // GET: api/ClientReminders/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ClientReminder>> GetClientReminder(int id)
        {
            var clientReminder = await _context.ClientReminders
                .Include(r => r.Client)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (clientReminder == null)
            {
                return NotFound();
            }

            return clientReminder;
        }

        // GET: api/ClientReminders/client/5
        [HttpGet("client/{clientId}")]
        public async Task<ActionResult<ClientReminder>> GetClientReminderByClient(int clientId)
        {
            var clientReminder = await _context.ClientReminders
                .Include(r => r.Client)
                .FirstOrDefaultAsync(r => r.Clientid == clientId);

            if (clientReminder == null)
            {
                return NotFound();
            }

            return clientReminder;
        }

        // GET: api/ClientReminders/upcoming
        [HttpGet("upcoming")]
        public async Task<ActionResult<IEnumerable<ClientReminder>>> GetUpcomingReminders()
        {
            var now = DateTime.Now;
            var endDate = now.AddDays(7); // Get reminders for next 7 days

            return await _context.ClientReminders
                .Where(r => r.ReminderDate >= now && r.ReminderDate <= endDate)
                .Include(r => r.Client)
                .OrderBy(r => r.ReminderDate)
                .ToListAsync();
        }

        // POST: api/ClientReminders
        [HttpPost]
        public async Task<ActionResult<ClientReminder>> PostClientReminder(ClientReminder clientReminder)
        {
            // Ensure client exists
            var clientExists = await _context.ClientProfiles.AnyAsync(c => c.Id == clientReminder.Clientid);
            if (!clientExists)
            {
                return BadRequest("Client does not exist");
            }

            // Check if client already has a reminder
            var existingReminder = await _context.ClientReminders
                .FirstOrDefaultAsync(r => r.Clientid == clientReminder.Clientid);

            if (existingReminder != null)
            {
                return Conflict("Client already has a reminder. Use PUT to update it.");
            }

            _context.ClientReminders.Add(clientReminder);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetClientReminder", new { id = clientReminder.Id }, clientReminder);
        }

        // PUT: api/ClientReminders/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutClientReminder(int id, ClientReminder clientReminder)
        {
            if (id != clientReminder.Id)
            {
                return BadRequest();
            }

            _context.Entry(clientReminder).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ClientReminderExists(id))
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

        // PATCH: api/ClientReminders/5/reminder-date
        [HttpPatch("{id}/reminder-date")]
        public async Task<IActionResult> UpdateReminderDate(int id, [FromBody] DateTime? reminderDate)
        {
            var reminder = await _context.ClientReminders.FindAsync(id);
            if (reminder == null)
            {
                return NotFound();
            }

            reminder.ReminderDate = reminderDate;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PATCH: api/ClientReminders/5/notes
        [HttpPatch("{id}/notes")]
        public async Task<IActionResult> UpdateReminderNotes(int id, [FromBody] string notes)
        {
            var reminder = await _context.ClientReminders.FindAsync(id);
            if (reminder == null)
            {
                return NotFound();
            }

            reminder.Notes = notes;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/ClientReminders/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteClientReminder(int id)
        {
            var clientReminder = await _context.ClientReminders.FindAsync(id);
            if (clientReminder == null)
            {
                return NotFound();
            }

            _context.ClientReminders.Remove(clientReminder);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ClientReminderExists(int id)
        {
            return _context.ClientReminders.Any(e => e.Id == id);
        }
    }
}
