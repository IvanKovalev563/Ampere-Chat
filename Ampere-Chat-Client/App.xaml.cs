using Ampere_Chat_Client.Services;

namespace Ampere_Chat_Client
{
    public partial class App : Application
    {
        private ApiService _apiService;
        private bool _isInitialized = false;

        // Событие для уведомления страниц о восстановлении сети
        public static event EventHandler NetworkRestored;

        public App()
        {
            InitializeComponent();

            // Страница загрузки с крутяшкой
            var loadingPage = new ContentPage
            {
                BackgroundColor = Colors.White,
                Content = new Grid
                {
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Center,
                    Children =
                    {
                        new ActivityIndicator
                        {
                            IsRunning = true,
                            IsVisible = true,
                            Color = Colors.Blue,
                            WidthRequest = 50,
                            HeightRequest = 50
                        },
                        new Label
                        {
                            Text = "Loading...",
                            FontSize = 18,
                            TextColor = Colors.Gray,
                            Margin = new Thickness(0, 100, 0, 0)
                        }
                    }
                }
            };

            MainPage = loadingPage;
            Connectivity.Current.ConnectivityChanged += OnConnectivityChanged;
        }

        protected override async void OnStart()
        {
            base.OnStart();

            if (_isInitialized) return;

            if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
            {
                await InitializeApp();
            }
        }

        private async Task InitializeApp()
        {
            if (_isInitialized) return;
            _isInitialized = true;

            _apiService = new ApiService();
            var token = Preferences.Get("auth_token", string.Empty);

            if (!string.IsNullOrEmpty(token))
            {
                _apiService.SetToken(token);
                AppState.ApiService = _apiService;

                try
                {
                    AppState.SignalRService = new SignalRService();
                    await AppState.SignalRService.ConnectAsync(token);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"SignalR init error: {ex.Message}");
                }

                if (DeviceInfo.Idiom == DeviceIdiom.Desktop || DeviceInfo.Idiom == DeviceIdiom.Tablet)
                    MainPage = new NavigationPage(new DesktopMainPage(_apiService));
                else
                    MainPage = new AppShell();
            }
            else
            {
                MainPage = new NavigationPage(new LoginPage(_apiService));
            }

            // После инициализации уведомляем о том, что сеть есть (если нужна перезагрузка данных)
            NetworkRestored?.Invoke(this, EventArgs.Empty);
        }

        private async void OnConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
        {
            if (e.NetworkAccess == NetworkAccess.Internet)
            {
                if (!_isInitialized)
                {
                    await InitializeApp();
                }
                else
                {
                    // Если приложение уже инициализировано, уведомляем страницы
                    NetworkRestored?.Invoke(this, EventArgs.Empty);
                }
            }
        }
    }

    public static class AppState
    {
        public static ApiService ApiService { get; set; }
        public static SignalRService SignalRService { get; set; }
    }
}