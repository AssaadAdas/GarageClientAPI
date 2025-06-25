using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using GarageClientAPI.Data;
using GarageClientAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GarageClientAPI.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class UserTypesController : ControllerBase
    {
        private readonly GarageClientContext _context;

        public UserTypesController(GarageClientContext context)
        {
            _context = context;
        }

        // GET: api/UserTypes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserType>>> GetUserTypes()
        {
            return await _context.UserTypes
                .OrderBy(ut => ut.Id)
                .ToListAsync();
        }

        // GET: api/UserTypes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UserType>> GetUserType(int id)
        {
            var userType = await _context.UserTypes
                //.Include(ut => ut.Users)
                //.Include(ut => ut.PremiumOffers)
                .FirstOrDefaultAsync(ut => ut.Id == id);

            if (userType == null)
            {
                return NotFound();
            }

            return userType;
        }

        // GET: api/UserTypes/5/users
        [HttpGet("{id}/users")]
        public async Task<ActionResult<IEnumerable<User>>> GetUsersByType(int id)
        {
            return await _context.Users
                .Where(u => u.UserTypeid == id)
                .OrderBy(u => u.Username)
                .ToListAsync();
        }

        // GET: api/UserTypes/5/offers
        [HttpGet("{id}/offers")]
        public async Task<ActionResult<IEnumerable<PremiumOffer>>> GetOffersByType(int id)
        {
            return await _context.PremiumOffers
                .Where(po => po.UserTypeid == id)
                .Include(po => po.Curr)
                .OrderBy(po => po.PremiumCost)
                .ToListAsync();
        }

        // POST: api/UserTypes
        [HttpPost]
        public async Task<ActionResult<UserType>> PostUserType(UserType userType)
        {
            // Validate description is unique if provided
            if (!string.IsNullOrEmpty(userType.UserTypeDesc) &&
                await _context.UserTypes.AnyAsync(ut => ut.UserTypeDesc == userType.UserTypeDesc))
            {
                return Conflict("A user type with this description already exists");
            }

            _context.UserTypes.Add(userType);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUserType", new { id = userType.Id }, userType);
        }

        // PUT: api/UserTypes/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUserType(int id, UserType userType)
        {
            if (id != userType.Id)
            {
                return BadRequest();
            }

            // Validate description is unique if provided (excluding current user type)
            if (!string.IsNullOrEmpty(userType.UserTypeDesc) &&
                await _context.UserTypes.AnyAsync(ut => ut.UserTypeDesc == userType.UserTypeDesc && ut.Id != id))
            {
                return Conflict("A user type with this description already exists");
            }

            _context.Entry(userType).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserTypeExists(id))
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

        // DELETE: api/UserTypes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserType(int id)
        {
            var userType = await _context.UserTypes.FindAsync(id);
            if (userType == null)
            {
                return NotFound();
            }

            // Check if user type is in use by users or premium offers
            if (await _context.Users.AnyAsync(u => u.UserTypeid == id) ||
                await _context.PremiumOffers.AnyAsync(po => po.UserTypeid == id))
            {
                return BadRequest("Cannot delete user type as it is being used by users or premium offers");
            }

            _context.UserTypes.Remove(userType);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserTypeExists(int id)
        {
            return _context.UserTypes.Any(e => e.Id == id);
        }
    }
}
