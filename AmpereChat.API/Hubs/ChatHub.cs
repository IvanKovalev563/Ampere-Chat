using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace AmpereChat.API.Hubs
{
    public class ChatHub : Hub
    {
        // Отправить сообщение всем в чате
        public async Task SendMessageToChat(int chatId, object message)
        {
            // Группируем клиентов по chatId, чтобы рассылать только нужным
            await Clients.Group($"chat_{chatId}").SendAsync("ReceiveMessage", message);
        }

        // Добавить клиента в группу чата (при подключении)
        public async Task JoinChat(int chatId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"chat_{chatId}");
        }

        // Удалить из группы при отключении
        public async Task LeaveChat(int chatId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"chat_{chatId}");
        }
    }
}