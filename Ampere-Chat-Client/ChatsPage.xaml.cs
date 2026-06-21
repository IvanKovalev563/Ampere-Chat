using Ampere_Chat_Client.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Ampere_Chat_Client
{
    public partial class ChatsPage : ContentPage
    {
        public event EventHandler<ChatDto> ChatSelected;
        private readonly ApiService _apiService;
        public ObservableCollection<ChatDto> Chats { get; set; } = new();

        public ChatsPage()
        {
            InitializeComponent();
            _apiService = AppState.ApiService ?? throw new Exception("ApiService not initialized");
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
                ChatsCollectionView.SelectedItem = null;
                ChatSelected?.Invoke(this, selectedChat);
                // Если на мобилке — переходим на ChatPage
                if (DeviceInfo.Idiom == DeviceIdiom.Phone)
                    await Navigation.PushAsync(new ChatPage(_apiService, selectedChat.chat_id, selectedChat.display_name));
            }
        }
        private async void OnNewChatClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new NewChatPage(_apiService));
        }
    }
}