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
    public class ClientPaymentOrdersController : ControllerBase
    {
        private readonly GarageClientContext _context;

        public ClientPaymentOrdersController(GarageClientContext context)
        {
            _context = context;
        }

        // GET: api/ClientPaymentOrders
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ClientPaymentOrder>>> GetClientPaymentOrders()
        {
            return await _context.ClientPaymentOrders
                .Include(o => o.Client)
                .Include(o => o.Curr)
                .Include(o => o.PremiumOffer)
                .ToListAsync();
        }

        // GET: api/ClientPaymentOrders/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ClientPaymentOrder>> GetClientPaymentOrder(int id)
        {
            var clientPaymentOrder = await _context.ClientPaymentOrders
                .Include(o => o.Client)
                .Include(o => o.Curr)
                .Include(o => o.PremiumOffer)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (clientPaymentOrder == null)
            {
                return NotFound();
            }

            return clientPaymentOrder;
        }

        // GET: api/ClientPaymentOrders/client/5
        [HttpGet("client/{clientId}")]
        public async Task<ActionResult<IEnumerable<ClientPaymentOrder>>> GetOrdersByClient(int clientId)
        {
            return await _context.ClientPaymentOrders
                .Where(o => o.ClientId == clientId)
                .Include(o => o.Client)
                .Include(o => o.Curr)
                .Include(o => o.PremiumOffer)
                .OrderByDescending(o => o.CreatedDate)
                .ToListAsync();
        }

        // GET: api/ClientPaymentOrders/status/{status}
        [HttpGet("status/{status}")]
        public async Task<ActionResult<IEnumerable<ClientPaymentOrder>>> GetOrdersByStatus(string status)
        {
            return await _context.ClientPaymentOrders
                .Where(o => o.Status == status)
                .Include(o => o.Client)
                .Include(o => o.Curr)
                .Include(o => o.PremiumOffer)
                .OrderByDescending(o => o.CreatedDate)
                .ToListAsync();
        }

        // GET: api/ClientPaymentOrders/method/5
        [HttpGet("method/{paymentMethodId}")]
        public async Task<ActionResult<IEnumerable<ClientPaymentOrder>>> GetOrdersByPaymentMethod(int paymentMethodId)
        {
            return await _context.ClientPaymentOrders
                .Where(o => o.PaymentMethodId == paymentMethodId)
                .Include(o => o.Client)
                .Include(o => o.Curr)
                .Include(o => o.PremiumOffer)
                .OrderByDescending(o => o.CreatedDate)
                .ToListAsync();
        }

        // POST: api/ClientPaymentOrders
        [HttpPost]
        public async Task<ActionResult<ClientPaymentOrder>> PostClientPaymentOrder(ClientPaymentOrder clientPaymentOrder)
        {
            // Set default values
            clientPaymentOrder.CreatedDate = DateTime.Now;
            clientPaymentOrder.Status = "Pending";
            clientPaymentOrder.OrderNumber = GenerateOrderNumber();

            _context.ClientPaymentOrders.Add(clientPaymentOrder);
            await _context.SaveChangesAsync();

            // Process payment in background (example)
            _ = ProcessPaymentAsync(clientPaymentOrder.Id);

            return CreatedAtAction("GetClientPaymentOrder", new { id = clientPaymentOrder.Id }, clientPaymentOrder);
        }

        // PATCH: api/ClientPaymentOrders/5/status
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] string status)
        {
            var order = await _context.ClientPaymentOrders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            order.Status = status;

            if (status == "Processed")
            {
                order.ProcessedDate = DateTime.Now;
            }

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/ClientPaymentOrders/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteClientPaymentOrder(int id)
        {
            var clientPaymentOrder = await _context.ClientPaymentOrders.FindAsync(id);
            if (clientPaymentOrder == null)
            {
                return NotFound();
            }

            _context.ClientPaymentOrders.Remove(clientPaymentOrder);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ClientPaymentOrderExists(int id)
        {
            return _context.ClientPaymentOrders.Any(e => e.Id == id);
        }

        private string GenerateOrderNumber()
        {
            return $"ORD-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
        }

        private async Task ProcessPaymentAsync(int orderId)
        {
            // Simulate payment processing delay
            await Task.Delay(5000);

            var order = await _context.ClientPaymentOrders.FindAsync(orderId);
            if (order != null && order.Status == "Pending")
            {
                order.Status = "Processed";
                order.ProcessedDate = DateTime.Now;
                await _context.SaveChangesAsync();

                // Here you would typically:
                // 1. Call your payment gateway
                // 2. Handle the response
                // 3. Update the order status accordingly
                // 4. Possibly create a premium registration if payment succeeds
            }
        }
    }
}
