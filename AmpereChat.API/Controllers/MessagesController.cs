using AmpereChat.API.Models;
using AmpereChat.API.Repositories;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Security.Claims;

namespace AmpereChat.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class MessagesController : ControllerBase
    {
        private readonly IConfiguration _config;
        private AppRepository _repo;
        public MessagesController(IConfiguration config, AppRepository repo)
        {
            _config = config;
            _repo = repo;
        }


        [HttpGet]
        // GET /api/messages?chatId=1
        public async Task<IActionResult> GetMessages([FromQuery] int chatId)
        {
            try
            {
                using var connection = new MySqlConnection(
                    _config.GetConnectionString("DefaultConnection"));
                var sql = @"SELECT message_id, chat_id, user_id, text, sent_at 
                    FROM Message 
                    WHERE chat_id = @chatId 
                    ORDER BY sent_at DESC 
                    LIMIT 50";
                var messages = await connection.QueryAsync<Message>(sql, new { chatId });
                return Ok(messages);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        
        [HttpPost]
        // POST /api/messages
        public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();
            int userId = int.Parse(userIdClaim.Value);

            // Chat check
            var chat = await _repo.GetChatById(request.ChatId);
            if (chat == null)
                return BadRequest("Чат не найден");

            // Временно userId берём из запроса (потом заменим на токен)
            var messageId = await _repo.AddMessage(request.ChatId, userId, request.Text);
            return Ok(new { messageId, message = "Сообщение отправлено" });
        }

        public class SendMessageRequest
        {
            public int ChatId { get; set; }
            public string Text { get; set; } = "";
        }
    }
}
