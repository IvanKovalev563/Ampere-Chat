using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;

namespace Ampere_Chat_Client.Services
{
    public class SignalRService : IDisposable
    {
        private HubConnection _hubConnection;
        private string _token;
        public event Action<MessageDto> OnMessageReceived;
        public event Action OnReconnecting;
        public event Action OnReconnected;
        public event Action<string> OnError;

        public async Task ConnectAsync(string token)
        {
            _token = token;
            if (_hubConnection != null && _hubConnection.State == HubConnectionState.Connected)
                return;

            if (_hubConnection != null)
            {
                await _hubConnection.DisposeAsync();
                _hubConnection = null;
            }

            _hubConnection = new HubConnectionBuilder()
                .WithUrl("http://ampere-chat.runasp.net/chathub", options =>
                {
                    options.AccessTokenProvider = () => Task.FromResult(token);
                })
                .WithAutomaticReconnect(new RetryPolicy())
                .ConfigureLogging(logging =>
                {
                    logging.SetMinimumLevel(LogLevel.Information);
                })
                .Build();

            _hubConnection.On<MessageDto>("ReceiveMessage", (message) =>
            {
                OnMessageReceived?.Invoke(message);
            });

            // Обработка разрыва соединения
            _hubConnection.Closed += async (error) =>
            {
                OnError?.Invoke(error?.Message ?? "Соединение закрыто");
                // Попытка переподключения уже обрабатывается WithAutomaticReconnect
            };

            _hubConnection.Reconnecting += (error) =>
            {
                OnReconnecting?.Invoke();
                return Task.CompletedTask;
            };

            _hubConnection.Reconnected += (connectionId) =>
            {
                OnReconnected?.Invoke();
                return Task.CompletedTask;
            };

            try
            {
                await _hubConnection.StartAsync();
            }
            catch (Exception ex)
            {
                OnError?.Invoke($"Ошибка подключения: {ex.Message}");
                throw;
            }
        }

        public async Task JoinChat(int chatId)
        {
            if (_hubConnection?.State == HubConnectionState.Connected)
            {
                await _hubConnection.InvokeAsync("JoinChat", chatId);
            }
        }

        public async Task LeaveChat(int chatId)
        {
            if (_hubConnection?.State == HubConnectionState.Connected)
            {
                await _hubConnection.InvokeAsync("LeaveChat", chatId);
            }
        }

        public async Task DisconnectAsync()
        {
            if (_hubConnection != null)
            {
                await _hubConnection.DisposeAsync();
                _hubConnection = null;
            }
        }

        public void Dispose()
        {
            _hubConnection?.DisposeAsync();
        }
    }

    // Политика автоматического переподключения
    public class RetryPolicy : IRetryPolicy
    {
        public TimeSpan? NextRetryDelay(RetryContext context)
        {
            // Экспоненциальная задержка: 0, 2, 10, 30 секунд
            if (context.PreviousRetryCount == 0)
                return TimeSpan.FromSeconds(1);
            if (context.PreviousRetryCount == 1)
                return TimeSpan.FromSeconds(5);
            if (context.PreviousRetryCount == 2)
                return TimeSpan.FromSeconds(15);
            return TimeSpan.FromSeconds(30);
        }
    }
}