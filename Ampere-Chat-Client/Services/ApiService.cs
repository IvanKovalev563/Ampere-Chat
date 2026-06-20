using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Maui.Storage;

namespace Ampere_Chat_Client.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "http://ampere-chat.runasp.net"; // замени на свой URL

        public ApiService()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(BaseUrl)
            };
        }

        // Установить токен после логина
        public void SetToken(string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        // Регистрация
        public async Task<bool> RegisterAsync(string username, string password)
        {
            var response = await _httpClient.PostAsJsonAsync("/api/auth/register",
                new { username, password });
            return response.IsSuccessStatusCode;
        }

        // Логин
        public async Task<string?> LoginAsync(string username, string password)
        {
            var response = await _httpClient.PostAsJsonAsync("/api/auth/login",
                new { username, password });
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.GetProperty("token").GetString();
        }

        // Получить список чатов (требует токен)
        public async Task<List<ChatDto>> GetChatsAsync()
        {
            var response = await _httpClient.GetAsync("/api/chats");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<ChatDto>>() ?? new();
        }

        // Получить сообщения чата
        public async Task<List<MessageDto>> GetMessagesAsync(int chatId)
        {
            var response = await _httpClient.GetAsync($"/api/messages?chatId={chatId}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<MessageDto>>() ?? new();
        }

        // Отправить сообщение
        public async Task<bool> SendMessageAsync(int chatId, string text)
        {
            var response = await _httpClient.PostAsJsonAsync("/api/messages",
                new { chatId, text });
            return response.IsSuccessStatusCode;
        }
    }

    // DTO для чатов
    public class ChatDto
    {
        public int chat_id { get; set; }
        public string display_name { get; set; } = "";
        public string last_message { get; set; } = "";
        public DateTime last_message_time { get; set; }
        public bool is_group { get; set; }
    }

    // DTO для сообщений
    public class MessageDto
    {
        public int message_id { get; set; }
        public int user_id { get; set; }
        public string text { get; set; } = "";
        public DateTime sent_at { get; set; }
        public string username { get; set; } = ""; // если сервер отдаёт
    }
}