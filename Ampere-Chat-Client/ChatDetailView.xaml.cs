using Ampere_Chat_Client.Services;
using System.Collections.ObjectModel;

namespace Ampere_Chat_Client
{
    public partial class ChatDetailView : ContentView
    {
        private readonly ApiService _apiService;
        private readonly int _chatId;
        private SignalRService _signalRService;
        public ObservableCollection<MessageDto> Messages { get; set; } = new();
        public int CurrentUserId { get; set; }
        private bool _isLoading = false;

        public ChatDetailView(ApiService apiService, int chatId)
        {
            InitializeComponent();
            _apiService = apiService;
            _chatId = chatId;
            CurrentUserId = AppState.ApiService?.GetCurrentUserId() ?? 0;
            BindingContext = this;

            _signalRService = AppState.SignalRService;
            if (_signalRService != null)
            {
                _signalRService.OnMessageReceived += OnNewMessage;
                _ = _signalRService.JoinChat(chatId);
            }

            this.Unloaded += (s, e) =>
            {
                if (_signalRService != null)
                {
                    _signalRService.OnMessageReceived -= OnNewMessage;
                    _ = _signalRService.LeaveChat(_chatId);
                }
            };
        }

        private void OnNewMessage(MessageDto message)
        {
            if (message.chat_id != _chatId) return;
            MainThread.BeginInvokeOnMainThread(() =>
            {
                InsertMessage(message);
                ScrollToEnd();
            });
        }

        private void InsertMessage(MessageDto message)
        {
            // Проверяем, нет ли уже такого ID
            var existing = Messages.FirstOrDefault(m => m.message_id == message.message_id);
            if (existing != null)
            {
                var idx = Messages.IndexOf(existing);
                Messages[idx] = message;
                return;
            }

            // Вставляем по времени
            int insertIndex = 0;
            for (int i = Messages.Count - 1; i >= 0; i--)
            {
                if (Messages[i].sent_at <= message.sent_at)
                {
                    insertIndex = i + 1;
                    break;
                }
            }
            Messages.Insert(insertIndex, message);
        }

        private void ScrollToEnd()
        {
            if (Messages.Count == 0) return;
            try
            {
                var last = Messages.OrderBy(m => m.sent_at).LastOrDefault();
                if (last != null)
                {
                    var idx = Messages.IndexOf(last);
                    if (idx >= 0)
                        MessagesCollectionView.ScrollTo(idx, position: ScrollToPosition.End, animate: false);
                }
            }
            catch { }
        }

        public async Task LoadMessages()
        {
            if (_isLoading) return;
            _isLoading = true;
            try
            {
                if (!ApiService.IsNetworkAvailable())
                {
                    await Application.Current.MainPage.DisplayAlert("Нет сети", "Проверьте подключение к интернету", "ОК");
                    return;
                }
                var list = await _apiService.GetMessagesAsync(_chatId);
                Messages.Clear();
                foreach (var msg in list.OrderBy(m => m.sent_at))
                    Messages.Add(msg);
                ScrollToEnd();
            }
            catch (HttpRequestException)
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка сети", "Не удалось загрузить сообщения. Проверьте интернет.", "ОК");
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка", $"Не удалось загрузить сообщения: {ex.Message}", "ОК");
            }
            finally { _isLoading = false; }
        }

        private async void OnSendClicked(object sender, EventArgs e) => await SendMessage();
        private async void OnMessageEntryCompleted(object sender, EventArgs e) => await SendMessage();

        private async Task SendMessage()
        {
            var text = MessageEntry.Text?.Trim();
            if (string.IsNullOrEmpty(text)) return;

            try
            {
                var success = await _apiService.SendMessageAsync(_chatId, text);
                if (success)
                {
                    MessageEntry.Text = "";
                    // Ждём SignalR — сообщение придёт само
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Ошибка", "Не удалось отправить", "ОК");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка", ex.Message, "ОК");
            }
        }
    }
}