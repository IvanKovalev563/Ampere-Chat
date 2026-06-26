using Ampere_Chat_Client.Services;
using System.Collections.ObjectModel;

namespace Ampere_Chat_Client
{
    public partial class ChatsView : ContentView
    {
        private ApiService _apiService => AppState.ApiService;
        public ObservableCollection<ChatDto> Chats { get; set; } = new();
        public event EventHandler<ChatDto> ChatSelected;
        private bool _isLoading = false;

        public ChatsView()
        {
            InitializeComponent();
            BindingContext = this;
            App.NetworkRestored += OnNetworkRestored;

            this.Unloaded += (s, e) =>
            {
                App.NetworkRestored -= OnNetworkRestored;
            };
        }

        protected override void OnParentSet()
        {
            base.OnParentSet();
            if (Parent != null)
                _ = LoadChats();
        }

        private void OnNetworkRestored(object sender, EventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await LoadChats();
            });
        }

        public async Task LoadChats()
        {
            if (_isLoading) return;
            _isLoading = true;

            // Показываем крутяшку
            LoadingIndicator.IsRunning = true;
            LoadingIndicator.IsVisible = true;

            try
            {
                if (_apiService == null)
                {
                    await Application.Current.MainPage.DisplayAlert("Ошибка", "Сервис не инициализирован", "ОК");
                    return;
                }

                if (!ApiService.IsNetworkAvailable())
                {
                    await Application.Current.MainPage.DisplayAlert("Нет сети", "Проверьте подключение к интернету", "ОК");
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
                await Application.Current.MainPage.DisplayAlert("Ошибка сети", "Не удалось загрузить чаты. Проверьте интернет.", "ОК");
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка", $"Не удалось загрузить чаты: {ex.Message}", "ОК");
            }
            finally
            {
                LoadingIndicator.IsRunning = false;
                LoadingIndicator.IsVisible = false;
                _isLoading = false;
            }
        }

        private void OnChatSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is ChatDto selectedChat)
            {
                ChatsCollectionView.SelectedItem = null;
                ChatSelected?.Invoke(this, selectedChat);
            }
        }
    }
}