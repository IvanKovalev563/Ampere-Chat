using AmpereChat.API.Repositories;
using BCrypt.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Crypto.Generators;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AmpereChat.API.Controllers
{
    [ApiController]  
    [Route("api/[controller]")]
    public class AuthController : Controller
    {
        private readonly AppRepository _repo;
        private IConfiguration _configuration;
        public AuthController(IConfiguration config, AppRepository repo)
        {
            _repo = repo;
            _configuration = config;
        }

        // POST /api/auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            // Password exist check
            var existing = await _repo.GetUserByUsername(request.Username);
            if (existing != null)
                return BadRequest("Пользователь с таким именем уже существует");

            // Password hash
            string hash = BCrypt.Net.BCrypt.HashPassword(request.Password);
            var userId = await _repo.CreateUser(request.Username, hash);
            return Ok(new { userId, message = "Регистрация успешна" });
        }

        // POST /api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var user = await _repo.GetUserByUsername(request.Username);
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.password_hash))
                return Unauthorized(new { message = "Неверное имя пользователя или пароль" });

            // Генерация JWT
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["JwtSettings:SecretKey"]!);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, user.user_id.ToString()) }),
                Expires = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["JwtSettings:ExpirationMinutes"])),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return Ok(new { token = tokenString, username = user.username });
        }
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _repo.GetAllUsers();
            return Ok(users);
        }
    }
    public class RegisterRequest
    {
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
    }

    public class LoginRequest
    {
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
    }
}
