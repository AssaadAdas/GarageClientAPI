using Microsoft.AspNetCore.Mvc;

namespace GarageClientAPI.Controllers
{
    using GarageClientAPI.Data;
    using GarageClientAPI.Models;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    [ApiController]
    [Route("api/[controller]")]
    public class PaymentTypesController : ControllerBase
    {
        private readonly GarageClientContext _context;

        public PaymentTypesController(GarageClientContext context)
        {
            _context = context;
        }

        // GET: api/PaymentTypes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PaymentType>>> GetPaymentTypes()
        {
            try
            {
                var paymentTypes = await _context.PaymentTypes.ToListAsync();
                return Ok(paymentTypes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/PaymentTypes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PaymentType>> GetPaymentType(int id)
        {
            try
            {
                var paymentType = await _context.PaymentTypes.FindAsync(id);

                if (paymentType == null)
                {
                    return NotFound($"Payment type with ID {id} not found");
                }

                return paymentType;
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/PaymentTypes/5/garage-payment-methods
        [HttpGet("{id}/garage-payment-methods")]
        public async Task<ActionResult<IEnumerable<GaragePaymentMethod>>> GetPaymentTypeGaragePaymentMethods(int id)
        {
            try
            {
                var paymentType = await _context.PaymentTypes
                    .Include(pt => pt.GaragePaymentMethods)
                    .FirstOrDefaultAsync(pt => pt.Id == id);

                if (paymentType == null)
                {
                    return NotFound($"Payment type with ID {id} not found");
                }

                return Ok(paymentType.GaragePaymentMethods);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/PaymentTypes/with-garage-methods
        [HttpGet("with-garage-methods")]
        public async Task<ActionResult<IEnumerable<PaymentType>>> GetPaymentTypesWithGarageMethods()
        {
            try
            {
                var paymentTypes = await _context.PaymentTypes
                    .Include(pt => pt.GaragePaymentMethods)
                    .ToListAsync();

                return Ok(paymentTypes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // POST: api/PaymentTypes
        [HttpPost]
        public async Task<ActionResult<PaymentType>> PostPaymentType(PaymentType paymentType)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Check if description is provided
                if (string.IsNullOrWhiteSpace(paymentType.PaymentTypeDesc))
                {
                    return BadRequest("Payment type description is required");
                }

                _context.PaymentTypes.Add(paymentType);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetPaymentType), new { id = paymentType.Id }, paymentType);
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, $"Error saving to database: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // PUT: api/PaymentTypes/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPaymentType(int id, PaymentType paymentType)
        {
            try
            {
                if (id != paymentType.Id)
                {
                    return BadRequest("ID in URL does not match ID in body");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (string.IsNullOrWhiteSpace(paymentType.PaymentTypeDesc))
                {
                    return BadRequest("Payment type description is required");
                }

                _context.Entry(paymentType).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PaymentTypeExists(id))
                    {
                        return NotFound($"Payment type with ID {id} not found");
                    }
                    else
                    {
                        throw;
                    }
                }

                return NoContent();
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, $"Error updating database: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // PATCH: api/PaymentTypes/5
        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchPaymentType(int id, [FromBody] Dictionary<string, object> updates)
        {
            try
            {
                var paymentType = await _context.PaymentTypes.FindAsync(id);
                if (paymentType == null)
                {
                    return NotFound($"Payment type with ID {id} not found");
                }

                // Update only the properties that are provided
                if (updates.ContainsKey("PaymentTypeDesc"))
                {
                    var description = updates["PaymentTypeDesc"]?.ToString();
                    if (string.IsNullOrWhiteSpace(description))
                    {
                        return BadRequest("Payment type description cannot be empty");
                    }
                    paymentType.PaymentTypeDesc = description;
                }

                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, $"Error updating database: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // DELETE: api/PaymentTypes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePaymentType(int id)
        {
            try
            {
                var paymentType = await _context.PaymentTypes.FindAsync(id);
                if (paymentType == null)
                {
                    return NotFound($"Payment type with ID {id} not found");
                }

                _context.PaymentTypes.Remove(paymentType);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, $"Error deleting from database: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        private bool PaymentTypeExists(int id)
        {
            return _context.PaymentTypes.Any(e => e.Id == id);
        }
    }
}
