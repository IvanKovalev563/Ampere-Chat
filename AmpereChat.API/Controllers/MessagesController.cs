using AmpereChat.API.Models;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace AmpereChat.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MessagesController : ControllerBase
    {
        private readonly IConfiguration _config;
        public MessagesController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet]
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
    }
}
