using Ampere_Chat_Client.Services;
using System.Collections.ObjectModel;

namespace Ampere_Chat_Client
{
    public partial class ChatsPage : ContentPage
    {
        public event EventHandler<ChatDto> ChatSelected;
        private readonly ApiService _apiService;
        public ObservableCollection<ChatDto> Chats { get; set; } = new();
        private bool _isLoading = false;

        public ChatsPage()
        {
            InitializeComponent();
            _apiService = AppState.ApiService;
            if (_apiService == null)
            {
                Application.Current.MainPage = new NavigationPage(new LoginPage(new ApiService()));
                return;
            }
            BindingContext = this;
            App.NetworkRestored += OnNetworkRestored;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadChats();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            App.NetworkRestored -= OnNetworkRestored;
        }

        private void OnNetworkRestored(object sender, EventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await LoadChats();
            });
        }

        private async Task LoadChats()
        {
            if (_isLoading) return;
            _isLoading = true;
            
            LoadingIndicator.IsRunning = true;
            LoadingIndicator.IsVisible = true;

            try
            {
                if (!ApiService.IsNetworkAvailable())
                {
                    await DisplayAlert("Нет сети", "Проверьте подключение к интернету", "ОК");
                    return;
                }

                var chatList = await _apiService.GetChatsAsync();
                Chats.Clear();
                foreach (var chat in chatList)
                    Chats.Add(chat);
            }
            catch (TaskCanceledException)
            {
                System.Diagnostics.Debug.WriteLine("Загрузка чатов отменена");
            }
            catch (HttpRequestException)
            {
                await DisplayAlert("Ошибка сети", "Не удалось загрузить чаты. Проверьте интернет.", "ОК");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Не удалось загрузить чаты: {ex.Message}", "ОК");
            }
            finally
            {
                LoadingIndicator.IsRunning = false;
                LoadingIndicator.IsVisible = false;
                _isLoading = false;
            }
        }

        private async void OnChatSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is ChatDto selectedChat)
            {
                ChatsCollectionView.SelectedItem = null;
                ChatSelected?.Invoke(this, selectedChat);
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