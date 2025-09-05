using GarageClientAPI.Data;
using GarageClientAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GarageClientAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientNotificationsController : ControllerBase
    {
        private readonly GarageClientContext _context;

        public ClientNotificationsController(GarageClientContext context)
        {
            _context = context;
        }

        // GET: api/ClientNotifications
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ClientNotification>>> GetClientNotifications()
        {
            return await _context.ClientNotifications
                .Include(cn => cn.Client)
                .OrderByDescending(cn => cn.IsRead)
                .OrderByDescending(cn => cn.Id)
                .ToListAsync();
        }

        // GET: api/ClientNotifications/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ClientNotification>> GetClientNotification(int id)
        {
            var notification = await _context.ClientNotifications
                .Include(cn => cn.Client)
                .FirstOrDefaultAsync(cn => cn.Id == id);

            if (notification == null)
            {
                return NotFound();
            }

            return notification;
        }

        // GET: api/ClientNotifications/ByClient/5
        [HttpGet("ByClient/{clientId}")]
        public async Task<ActionResult<IEnumerable<ClientNotification>>> GetClientNotificationsByClient(int clientId)
        {
            return await _context.ClientNotifications
                .Where(cn => cn.Clientid == clientId)
                .OrderByDescending(cn => cn.Id)
                .ToListAsync();
        }

        // GET: api/ClientNotifications/Unread/5
        [HttpGet("Unread/{clientId}")]
        public async Task<ActionResult<IEnumerable<ClientNotification>>> GetUnreadNotifications(int clientId)
        {
            return await _context.ClientNotifications
                .Where(cn => cn.Clientid == clientId && cn.IsRead==false ) // Uncomment if you add IsRead field
                .OrderByDescending(cn => cn.Id)
                .ToListAsync();
        }

        // GET: api/ClientNotifications/Recent/5
        [HttpGet("Recent/{clientId}")]
        public async Task<ActionResult<IEnumerable<ClientNotification>>> GetRecentNotifications(int clientId, [FromQuery] int days = 7)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-days);
            return await _context.ClientNotifications
                .Where(cn => cn.Clientid == clientId /* && cn.CreatedDate >= cutoffDate */) // Uncomment if you add CreatedDate
                .OrderByDescending(cn => cn.Id)
                .Take(20)
                .ToListAsync();
        }
        
        // POST: api/ClientNotifications
        [HttpPost]
        public async Task<ActionResult<ClientNotification>> PostClientNotification(ClientNotification notification)
        {
            // Basic validation
            if (string.IsNullOrWhiteSpace(notification.Notes))
            {
                return BadRequest("Notification notes are required.");
            }

            // Validate client exists
            if (!_context.ClientProfiles.Any(c => c.Id == notification.Clientid))
            {
                return BadRequest("Specified Client does not exist.");
            }

            // Set default values
            /* notification.CreatedDate = DateTime.UtcNow; */ // Uncomment if you add CreatedDate
            /* notification.IsRead = false; */ // Uncomment if you add IsRead field

            _context.ClientNotifications.Add(notification);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetClientNotification), new { id = notification.Id }, notification);
        }

        // POST: api/ClientNotifications/Bulk
        [HttpPost("Bulk")]
        public async Task<ActionResult> PostBulkClientNotifications([FromBody] List<ClientNotification> notifications)
        {
            if (notifications == null || !notifications.Any())
            {
                return BadRequest("No notifications provided.");
            }

            // Validate all clients exist
            var clientIds = notifications.Select(n => n.Clientid).Distinct();
            var existingClientIds = await _context.ClientProfiles
                .Where(c => clientIds.Contains(c.Id))
                .Select(c => c.Id)
                .ToListAsync();

            var invalidClients = clientIds.Except(existingClientIds).ToList();
            if (invalidClients.Any())
            {
                return BadRequest($"Invalid client IDs: {string.Join(", ", invalidClients)}");
            }

            // Set default values
            var now = DateTime.UtcNow;
            foreach (var notification in notifications)
            {
                /* notification.CreatedDate = now; */ // Uncomment if you add CreatedDate
                /* notification.IsRead = false; */ // Uncomment if you add IsRead field
            }

            _context.ClientNotifications.AddRange(notifications);
            await _context.SaveChangesAsync();

            return Ok(new { Count = notifications.Count });
        }

        // PUT: api/ClientNotifications/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutClientNotification(int id, ClientNotification notification)
        {
            if (id != notification.Id)
            {
                return BadRequest();
            }

            // Basic validation
            if (string.IsNullOrWhiteSpace(notification.Notes))
            {
                return BadRequest("Notification notes are required.");
            }

            // Don't allow changing ClientId
            var existingNotification = await _context.ClientNotifications.AsNoTracking().FirstOrDefaultAsync(cn => cn.Id == id);
            if (existingNotification != null && existingNotification.Clientid != notification.Clientid)
            {
                return BadRequest("Cannot change Client association.");
            }

            _context.Entry(notification).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ClientNotificationExists(id))
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

        // PATCH: api/ClientNotifications/5/MarkAsRead
        [HttpPatch("{id}/MarkAsRead")]
        public async Task<IActionResult> MarkNotificationAsRead(int id)
        {
            var notification = await _context.ClientNotifications.FindAsync(id);
            if (notification == null)
            {
                return NotFound();
            }

            /* notification.IsRead = true; */ // Uncomment if you add IsRead field
            /* notification.ReadDate = DateTime.UtcNow; */ // Uncomment if you add ReadDate
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/ClientNotifications/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteClientNotification(int id)
        {
            var notification = await _context.ClientNotifications.FindAsync(id);
            if (notification == null)
            {
                return NotFound();
            }

            _context.ClientNotifications.Remove(notification);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/ClientNotifications/ByClient/5
        [HttpDelete("ByClient/{clientId}")]
        public async Task<IActionResult> DeleteAllClientNotifications(int clientId)
        {
            var notifications = await _context.ClientNotifications
                .Where(cn => cn.Clientid == clientId)
                .ToListAsync();

            if (!notifications.Any())
            {
                return NotFound("No notifications found for this client.");
            }

            _context.ClientNotifications.RemoveRange(notifications);
            await _context.SaveChangesAsync();

            return Ok(new { Count = notifications.Count });
        }

        private bool ClientNotificationExists(int id)
        {
            return _context.ClientNotifications.Any(e => e.Id == id);
        }
    }
}
