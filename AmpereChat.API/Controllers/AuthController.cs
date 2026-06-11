using AmpereChat.API.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Crypto.Generators;
using BCrypt.Net;

namespace AmpereChat.API.Controllers
{
    [ApiController]  
    [Route("api/[controller]")]
    public class AuthController : Controller
    {
        private readonly AppRepository _repo;
        public AuthController(AppRepository repo)
        {
            _repo = repo;
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
            if (user == null)
                return Unauthorized("Неверное имя пользователя или пароль");

            bool valid = BCrypt.Net.BCrypt.Verify(request.Password, user.password_hash);
            if (!valid)
                return Unauthorized("Неверное имя пользователя или пароль");

            // Returning userId temporary (JWT in future)
            return Ok(new { user.user_id, username = user.username });
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
