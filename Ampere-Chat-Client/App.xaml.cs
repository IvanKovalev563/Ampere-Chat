using Ampere_Chat_Client.Services;

namespace Ampere_Chat_Client
{
    public partial class App : Application
    {
        private ApiService _apiService;

        public App()
        {
            InitializeComponent();
            // На время загрузки показываем пустую страницу (можно заменить на Splash)
            MainPage = new ContentPage { Content = new Label { Text = "Loading..."} };
        }

        protected override async void OnStart()
        {
            base.OnStart();
            _apiService = new ApiService();
            var token = Preferences.Get("auth_token", string.Empty);

            if (!string.IsNullOrEmpty(token))
            {
                _apiService.SetToken(token);
                AppState.ApiService = _apiService;
                AppState.SignalRService = new SignalRService();
                await AppState.SignalRService.ConnectAsync(token);

                // Для Windows и планшетов — двухколоночный вид
                if (DeviceInfo.Idiom == DeviceIdiom.Desktop || DeviceInfo.Idiom == DeviceIdiom.Tablet)
                {
                    MainPage = new NavigationPage(new DesktopMainPage(_apiService));
                }
                else
                {
                    MainPage = new AppShell();
                }
            }
            else
            {
                MainPage = new NavigationPage(new LoginPage(_apiService));
            }
        }
    }

    // Вспомогательный статический класс для доступа к ApiService из любой точки
    public static class AppState
    {
        public static ApiService ApiService { get; set; }
        public static SignalRService SignalRService { get; set; }
    }
}
