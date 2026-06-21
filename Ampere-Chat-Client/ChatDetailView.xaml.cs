using Ampere_Chat_Client.Services;
using System.Collections.ObjectModel;

namespace Ampere_Chat_Client
{
    public partial class ChatDetailView : ContentView
    {
        private readonly ApiService _apiService;
        private int _chatId;
        public ObservableCollection<MessageDto> Messages { get; set; } = new();
        public int CurrentUserId { get; set; }

        public ChatDetailView(ApiService apiService, int chatId)
        {
            InitializeComponent();
            _apiService = apiService;
            _chatId = chatId;
            CurrentUserId = AppState.ApiService?.GetCurrentUserId() ?? 0;
            BindingContext = this;
        }

        public async Task LoadMessages()
        {
            try
            {
                var messageList = await _apiService.GetMessagesAsync(_chatId);
                Messages.Clear();
                foreach (var msg in messageList.OrderBy(m => m.sent_at))
                    Messages.Add(msg);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки сообщений: {ex.Message}");
            }
        }

        private async void OnSendClicked(object sender, EventArgs e)
        {
            await SendMessage();
        }

        private async void OnMessageEntryCompleted(object sender, EventArgs e)
        {
            await SendMessage();
        }

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
                    await LoadMessages();
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка", ex.Message, "ОК");
            }
        }
    }
}