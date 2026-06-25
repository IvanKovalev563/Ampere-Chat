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
            _apiService = AppState.ApiService;
            if (_apiService == null)
            {
                // Перенаправляем на логин, если сервис не инициализирован
                Application.Current.MainPage = new NavigationPage(new LoginPage(new ApiService()));
                return;
            }
            BindingContext = this;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadChats();
        }
        // В ChatsView.xaml.cs
        protected override void OnParentSet()
        {
            base.OnParentSet();
            if (Parent != null)
                LoadChats();
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
            catch (TaskCanceledException)
            {
                // Просто игнорируем — запрос отменён пользователем (переключение вкладки и т.п.)
                System.Diagnostics.Debug.WriteLine("Загрузка чатов отменена");
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
            if (_apiService == null)
            {
                await DisplayAlert("Ошибка", "Сервис не инициализирован. Перезайдите.", "ОК");
                Application.Current.MainPage = new NavigationPage(new LoginPage(new ApiService()));
                return;
            }
            await Navigation.PushAsync(new NewChatPage(_apiService));
        }
    }
}