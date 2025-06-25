using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
    public class GaragePaymentOrdersController : ControllerBase
    {
        private readonly GarageClientContext _context;

        public GaragePaymentOrdersController(GarageClientContext context)
        {
            _context = context;
        }

        // GET: api/GaragePaymentOrders
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GaragePaymentOrder>>> GetGaragePaymentOrders()
        {
            return await _context.GaragePaymentOrders
                .Include(o => o.Curr)
                .Include(o => o.Garage)
                .Include(o => o.PremiumOffer)
                .OrderByDescending(o => o.CreatedDate)
                .ToListAsync();
        }

        // GET: api/GaragePaymentOrders/5
        [HttpGet("{id}")]
        public async Task<ActionResult<GaragePaymentOrder>> GetGaragePaymentOrder(int id)
        {
            var order = await _context.GaragePaymentOrders
                .Include(o => o.Curr)
                .Include(o => o.Garage)
                .Include(o => o.PremiumOffer)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return order;
        }

        // GET: api/GaragePaymentOrders/garage/5
        [HttpGet("garage/{garageId}")]
        public async Task<ActionResult<IEnumerable<GaragePaymentOrder>>> GetOrdersByGarage(int garageId)
        {
            return await _context.GaragePaymentOrders
                .Where(o => o.GarageId == garageId)
                .Include(o => o.Curr)
                .Include(o => o.PremiumOffer)
                .OrderByDescending(o => o.CreatedDate)
                .ToListAsync();
        }

        // GET: api/GaragePaymentOrders/status/{status}
        [HttpGet("status/{status}")]
        public async Task<ActionResult<IEnumerable<GaragePaymentOrder>>> GetOrdersByStatus(string status)
        {
            return await _context.GaragePaymentOrders
                .Where(o => o.Status == status)
                .Include(o => o.Garage)
                .Include(o => o.Curr)
                .Include(o => o.PremiumOffer)
                .OrderByDescending(o => o.CreatedDate)
                .ToListAsync();
        }

        // GET: api/GaragePaymentOrders/method/5
        [HttpGet("method/{paymentMethodId}")]
        public async Task<ActionResult<IEnumerable<GaragePaymentOrder>>> GetOrdersByPaymentMethod(int paymentMethodId)
        {
            return await _context.GaragePaymentOrders
                .Where(o => o.PaymentMethodId == paymentMethodId)
                .Include(o => o.Garage)
                .Include(o => o.Curr)
                .Include(o => o.PremiumOffer)
                .OrderByDescending(o => o.CreatedDate)
                .ToListAsync();
        }

        // POST: api/GaragePaymentOrders
        [HttpPost]
        public async Task<ActionResult<GaragePaymentOrder>> PostGaragePaymentOrder(GaragePaymentOrder order)
        {
            // Set default values
            order.CreatedDate = DateTime.Now;
            order.Status = "Pending";
            order.OrderNumber = GenerateOrderNumber();

            _context.GaragePaymentOrders.Add(order);
            await _context.SaveChangesAsync();

            // Process payment in background
            _ = ProcessPaymentAsync(order.Id);

            return CreatedAtAction("GetGaragePaymentOrder", new { id = order.Id }, order);
        }

        // PATCH: api/GaragePaymentOrders/5/status
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] string status)
        {
            var order = await _context.GaragePaymentOrders.FindAsync(id);
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

        // DELETE: api/GaragePaymentOrders/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGaragePaymentOrder(int id)
        {
            var order = await _context.GaragePaymentOrders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            _context.GaragePaymentOrders.Remove(order);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool GaragePaymentOrderExists(int id)
        {
            return _context.GaragePaymentOrders.Any(e => e.Id == id);
        }

        private string GenerateOrderNumber()
        {
            return $"GPO-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
        }

        private async Task ProcessPaymentAsync(int orderId)
        {
            // Simulate payment processing delay
            await Task.Delay(5000);

            var order = await _context.GaragePaymentOrders
                .Include(o => o.Garage)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order != null && order.Status == "Pending")
            {
                // Get payment method
                var paymentMethod = await _context.GaragePaymentMethods
                    .FirstOrDefaultAsync(p => p.Id == order.PaymentMethodId);

                if (paymentMethod != null)
                {
                    try
                    {
                        // In a real implementation, you would call your payment gateway here
                        // For simulation, we'll just mark as processed
                        order.Status = "Processed";
                        order.ProcessedDate = DateTime.Now;

                        await _context.SaveChangesAsync();

                        // Here you would typically:
                        // 1. Call payment gateway
                        // 2. Handle response
                        // 3. Update order status accordingly
                        // 4. Process any premium benefits if payment succeeds
                    }
                    catch
                    {
                        order.Status = "Failed";
                        await _context.SaveChangesAsync();
                    }
                }
                else
                {
                    order.Status = "Failed - Invalid Payment Method";
                    await _context.SaveChangesAsync();
                }
            }
        }

        // GET: api/GaragePaymentOrders/summary/garage/5
        [HttpGet("summary/garage/{garageId}")]
        public async Task<ActionResult<object>> GetGaragePaymentSummary(int garageId)
        {
            var summary = new
            {
                TotalOrders = await _context.GaragePaymentOrders
                    .Where(o => o.GarageId == garageId)
                    .CountAsync(),
                TotalAmount = await _context.GaragePaymentOrders
                    .Where(o => o.GarageId == garageId)
                    .SumAsync(o => o.Amount),
                LastPayment = await _context.GaragePaymentOrders
                    .Where(o => o.GarageId == garageId)
                    .OrderByDescending(o => o.CreatedDate)
                    .FirstOrDefaultAsync()
            };

            return Ok(summary);
        }

        // GET: api/GaragePaymentOrders/recent
        [HttpGet("recent")]
        public async Task<ActionResult<IEnumerable<GaragePaymentOrder>>> GetRecentOrders()
        {
            return await _context.GaragePaymentOrders
                .Include(o => o.Garage)
                .Include(o => o.Curr)
                .Include(o => o.PremiumOffer)
                .OrderByDescending(o => o.CreatedDate)
                .Take(10)
                .ToListAsync();
        }
    }
}
