using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using server.Data;
using server.DTOs;
using server.Models;

namespace server.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AuthController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var isUserExists = await _context.Users.AnyAsync(u => u.Username == request.Username);
            if (isUserExists)
            {
                return BadRequest(new { message = "Username already exists" });
            }

            var newUser = new UserModel
            {
                Username = request.Username,
                Password = BCrypt.Net.BCrypt.HashPassword(request.Password)
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return Ok(new { message = "User registered successfully" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
            if (user == null)
            {
                return Unauthorized(new { message = "Invalid username or password" });
            }

            if(user.FailedLoginAttempts > 3)
            {
                return Unauthorized(new { message = "Account locked due to multiple failed login attempts", isLocked = true });
            }

            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.Password);
            if (!isPasswordValid)
            {
                user.FailedLoginAttempts++;
                await _context.SaveChangesAsync();

                if(user.FailedLoginAttempts > 3)
                {
                    return Unauthorized(new { message = "Account locked due to multiple failed login attempts", isLocked = true });
                }

                return Unauthorized(new { message = "Invalid username or password" });
            }

            user.FailedLoginAttempts = 0;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Login successful" });
        }
    }
}