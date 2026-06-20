using Ampere_Chat_Client.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Ampere_Chat_Client
{
    public partial class ChatsPage : ContentPage
    {
        private readonly ApiService _apiService;
        public ObservableCollection<ChatDto> Chats { get; set; } = new();

        public ChatsPage(ApiService apiService)
        {
            InitializeComponent();
            _apiService = apiService;
            BindingContext = this;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadChats();
        }

        private async Task LoadChats()
        {
            try
            {
                var chatList = await _apiService.GetChatsAsync();
                Chats.Clear();
                foreach (var chat in chatList)
                    Chats.Add(chat);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Не удалось загрузить чаты: {ex.Message}", "ОК");
            }
        }

        private async void OnChatSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is ChatDto selectedChat)
            {
                // Снимаем выделение
                ChatsCollectionView.SelectedItem = null;

                // Переход на страницу переписки (создадим позже)
                await DisplayAlert("Выбран чат", $"Чат с {selectedChat.display_name}", "ОК");
                // await Navigation.PushAsync(new ChatPage(_apiService, selectedChat.chat_id));
            }
        }
    }
}