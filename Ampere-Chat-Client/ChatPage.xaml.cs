using Ampere_Chat_Client.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Ampere_Chat_Client
{

    public partial class ChatPage : ContentPage
    {
        private readonly ApiService _apiService;
        private readonly int _chatId;
        public ObservableCollection<MessageDto> Messages { get; set; } = new();
        public string ChatName { get; set; }
        public int CurrentUserId { get; set; }
        public ChatPage(ApiService apiService, int chatId, string chatName)
        {
            InitializeComponent();
            _apiService = apiService;
            _chatId = chatId;
            ChatName = chatName;
            CurrentUserId = AppState.ApiService?.GetCurrentUserId() ?? 0;
            BindingContext = this;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadMessages();
        }

        private async Task LoadMessages()
        {
            try
            {
                var messageList = await _apiService.GetMessagesAsync(_chatId);
                Messages.Clear();
                foreach (var msg in messageList.OrderBy(m => m.sent_at))
                    Messages.Add(msg);
                foreach (var msg in messageList)
                {
                    System.Diagnostics.Debug.WriteLine($"Сообщение: {msg.text}, user_id: {msg.user_id}, CurrentUserId: {CurrentUserId}");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Не удалось загрузить сообщения: {ex.Message}", "ОК");
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
                    await LoadMessages(); // обновляем список после отправки
                }
                else
                {
                    await DisplayAlert("Ошибка", "Не удалось отправить сообщение", "ОК");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", ex.Message, "ОК");
            }
        }
    }
}