using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using AmpereChat.API.Repositories;

namespace AmpereChat.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ChatsController : ControllerBase
    {
        private readonly AppRepository _repo;
        public ChatsController(AppRepository repo)
        {
            _repo = repo;
        }

        // GET /api/chats – список чатов текущего пользователя
        [HttpGet]
        public async Task<IActionResult> GetChats()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();
            int userId = int.Parse(userIdClaim.Value);

            var chats = await _repo.GetChatsForUser(userId);
            return Ok(chats);
        }

        // POST /api/chats/private – создать личный чат с другим пользователем
        [HttpPost("private")]
        public async Task<IActionResult> CreatePrivateChat([FromBody] CreatePrivateChatRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();
            int currentUserId = int.Parse(userIdClaim.Value);

            // Проверяем, не существует ли уже чат между этими пользователями
            var existingChatId = await _repo.GetPrivateChatBetweenUsers(currentUserId, request.OtherUserId);
            if (existingChatId.HasValue)
                return Ok(new { chatId = existingChatId.Value, message = "Чат уже существует" });

            // Создаём новый
            var newChatId = await _repo.CreatePrivateChat(currentUserId, request.OtherUserId);
            return Ok(new { chatId = newChatId, message = "Чат создан" });
        }
    }

    // DTO для запроса создания личного чата
    public class CreatePrivateChatRequest
    {
        public int OtherUserId { get; set; } // ID собеседника
    }
}