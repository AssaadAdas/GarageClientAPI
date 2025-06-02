using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GarageClientAPI.Data;
using GarageClientAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace GarageClientAPI.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly GarageClientContext _context;
        private readonly IConfiguration _configuration;

        public UsersController(GarageClientContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // POST: api/Users/register
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult<User>> Register(User user)
        {
            // Validate required fields
            if (string.IsNullOrEmpty(user.Username) || string.IsNullOrEmpty(user.Password))
            {
                return BadRequest("Username and password are required");
            }

            // Validate username is unique
            if (await _context.Users.AnyAsync(u => u.Username == user.Username))
            {
                return Conflict("Username already exists");
            }

            // Validate user type exists
            if (!await _context.UserTypes.AnyAsync(ut => ut.Id == user.UserTypeid))
            {
                return BadRequest("Invalid user type");
            }

            // Hash password
            user.Password = HashPassword(user.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Don't return password hash
            user.Password = null;
            return CreatedAtAction("GetUser", new { id = user.Id }, user);
        }

        // POST: api/Users/login
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<object>> Login([FromBody] LoginRequest loginRequest)
        {
            var user = await _context.Users
                .Include(u => u.UserType)
                .FirstOrDefaultAsync(u => u.Username == loginRequest.Username);

            if (user == null || !VerifyPassword(loginRequest.Password, user.Password))
            {
                return Unauthorized("Invalid username or password");
            }

            // Generate JWT token
            var token = GenerateJwtToken(user);

            // Don't return password hash
            user.Password = null;

            return Ok(new
            {
                Token = token,
                User = user
            });
        }

        // GET: api/Users
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            var users = await _context.Users
                .Include(u => u.UserType)
                .ToListAsync();

            // Don't return password hashes
            users.ForEach(u => u.Password = null);
            return users;
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _context.Users
                .Include(u => u.UserType)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            // Only allow admins or the user themselves to access the details
            var currentUserId = int.Parse(User.FindFirst("id").Value);
            var isAdmin = User.IsInRole("Admin");

            if (!isAdmin && currentUserId != id)
            {
                return Forbid();
            }

            // Don't return password hash
            user.Password = null;
            return user;
        }

        // GET: api/Users/type/2
        [HttpGet("type/{userTypeId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<User>>> GetUsersByType(int userTypeId)
        {
            var users = await _context.Users
                .Where(u => u.UserTypeid == userTypeId)
                .Include(u => u.UserType)
                .ToListAsync();

            // Don't return password hashes
            users.ForEach(u => u.Password = null);
            return users;
        }

        // PUT: api/Users/5
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> PutUser(int id, User user)
        {
            if (id != user.Id)
            {
                return BadRequest();
            }

            // Only allow admins or the user themselves to update
            var currentUserId = int.Parse(User.FindFirst("id").Value);
            var isAdmin = User.IsInRole("Admin");

            if (!isAdmin && currentUserId != id)
            {
                return Forbid();
            }

            // Validate username is unique (excluding current user)
            if (await _context.Users.AnyAsync(u => u.Username == user.Username && u.Id != id))
            {
                return Conflict("Username already exists");
            }

            // Get existing user to preserve password if not being changed
            var existingUser = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
            if (existingUser == null)
            {
                return NotFound();
            }

            // Only admins can change user type
            if (!isAdmin && user.UserTypeid != existingUser.UserTypeid)
            {
                return Forbid();
            }

            // Validate user type exists
            if (!await _context.UserTypes.AnyAsync(ut => ut.Id == user.UserTypeid))
            {
                return BadRequest("Invalid user type");
            }

            // Preserve password if not being changed
            if (string.IsNullOrEmpty(user.Password))
            {
                user.Password = existingUser.Password;
            }
            else
            {
                // Hash new password
                user.Password = HashPassword(user.Password);
            }

            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
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

        // PATCH: api/Users/5/password
        [HttpPatch("{id}/password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword(int id, [FromBody] PasswordChangeRequest request)
        {
            // Only allow the user themselves to change password
            var currentUserId = int.Parse(User.FindFirst("id").Value);
            if (currentUserId != id)
            {
                return Forbid();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Verify old password
            if (!VerifyPassword(request.OldPassword, user.Password))
            {
                return BadRequest("Invalid current password");
            }

            // Update password
            user.Password = HashPassword(request.NewPassword);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Check if user has associated profiles
            if (await _context.ClientProfiles.AnyAsync(c => c.UserId == id) ||
                await _context.GarageProfiles.AnyAsync(g => g.UserId == id))
            {
                return BadRequest("Cannot delete user as they have associated profiles");
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }

        private string HashPassword(string password)
        {
            using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(_configuration["PasswordSalt"]));
            var passwordBytes = Encoding.UTF8.GetBytes(password);
            var hash = hmac.ComputeHash(passwordBytes);
            return Convert.ToBase64String(hash);
        }

        private bool VerifyPassword(string password, string storedHash)
        {
            var computedHash = HashPassword(password);
            return computedHash == storedHash;
        }

        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["JwtSecret"]);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                new Claim("id", user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.UserType?.UserTypeDesc ?? "User")
            }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }

    // Simple request classes (not full DTOs)
    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class PasswordChangeRequest
    {
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }
}
