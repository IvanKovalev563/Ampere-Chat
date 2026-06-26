using Ampere_Chat_Client.Services;
using Microsoft.Maui.Storage;

namespace Ampere_Chat_Client
{
    public partial class LoginPage : ContentPage
    {
        private readonly ApiService _apiService;

        public LoginPage(ApiService apiService)
        {
            InitializeComponent();
            _apiService = apiService;
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            var username = UsernameEntry.Text?.Trim();
            var password = PasswordEntry.Text;

            LoginButton.IsEnabled = false;
            LoginButton.Text = "";
            LoginIndicator.IsRunning = true;
            LoginIndicator.IsVisible = true;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ShowMessage("Введите логин и пароль");
                LoginButton.IsEnabled = true;
                LoginButton.Text = "Войти";
                LoginIndicator.IsRunning = false;
                LoginIndicator.IsVisible = false;
                return;
            }

            if (!ApiService.IsNetworkAvailable())
            {
                ShowMessage("Нет интернета. Проверьте подключение.");
                LoginButton.IsEnabled = true;
                LoginButton.Text = "Войти";
                LoginIndicator.IsRunning = false;
                LoginIndicator.IsVisible = false;
                return;
            }

            try
            {
                var token = await _apiService.LoginAsync(username, password);
                if (token == null)
                {
                    ShowMessage("Неверный логин или пароль");
                    LoginButton.IsEnabled = true;
                    LoginButton.Text = "Войти";
                    LoginIndicator.IsRunning = false;
                    LoginIndicator.IsVisible = false;
                    return;
                }

                Preferences.Set("auth_token", token);
                _apiService.SetToken(token);
                AppState.ApiService = _apiService;

                try
                {
                    AppState.SignalRService = new SignalRService();
                    await AppState.SignalRService.ConnectAsync(token);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"SignalR не подключился: {ex.Message}");
                    // Не блокируем вход, если SignalR не работает
                }

                // Переход на главную страницу
                if (DeviceInfo.Idiom == DeviceIdiom.Desktop || DeviceInfo.Idiom == DeviceIdiom.Tablet)
                    Application.Current.MainPage = new NavigationPage(new DesktopMainPage(_apiService));
                else
                    Application.Current.MainPage = new AppShell();
            }
            catch (Exception ex)
            {
                ShowMessage($"Ошибка входа: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Login error: {ex}");
            }
            finally
            {
                LoginButton.IsEnabled = true;
                LoginButton.Text = "Войти";
                LoginIndicator.IsRunning = false;
                LoginIndicator.IsVisible = false;
            }
        }

        private async void OnRegisterClicked(object sender, EventArgs e)
        {
            var username = UsernameEntry.Text?.Trim();
            var password = PasswordEntry.Text;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ShowMessage("Введите логин и пароль");
                return;
            }

            if (!ApiService.IsNetworkAvailable())
            {
                ShowMessage("Нет интернета. Проверьте подключение.");
                return;
            }

            try
            {
                var success = await _apiService.RegisterAsync(username, password);
                if (success)
                {
                    ShowMessage("Регистрация успешна! Теперь войдите.", false);
                }
                else
                {
                    ShowMessage("Ошибка регистрации. Возможно, имя занято.");
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Ошибка регистрации: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Register error: {ex}");
            }
        }

        private void ShowMessage(string text, bool isError = true)
        {
            MessageLabel.Text = text;
            MessageLabel.TextColor = isError ? Colors.Red : Colors.Green;
            MessageLabel.IsVisible = true;
        }
    }
}