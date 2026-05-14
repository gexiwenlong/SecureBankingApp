using Microsoft.AspNetCore.Mvc;
using SecureBankingApp.API.Models;
using SecureBankingApp.API.Services;

namespace SecureBankingApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private static List<User> _users = new List<User>();
        private readonly IPasswordService _passwordService;

        public AuthController(IPasswordService passwordService)
        {
            _passwordService = passwordService;
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterRequest request)
        {
            if (_users.Any(u => u.Email == request.Email))
                return BadRequest(new { error = "Email already registered" });

            var user = new User
            {
                Id = _users.Count + 1,
                Email = request.Email,
                PasswordHash = _passwordService.HashPassword(request.Password),
                FirstName = request.FirstName,
                LastName = request.LastName
            };

            _users.Add(user);
            return Ok(new { message = "Registration successful" });
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            var user = _users.FirstOrDefault(u => u.Email == request.Email);
            if (user == null)
                return Unauthorized(new { error = "Invalid email or password" });

            if (!_passwordService.VerifyPassword(request.Password, user.PasswordHash))
                return Unauthorized(new { error = "Invalid email or password" });

            return Ok(new { message = "Login successful", userId = user.Id, role = user.Role });
        }
    }

    public class RegisterRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
    }

    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
