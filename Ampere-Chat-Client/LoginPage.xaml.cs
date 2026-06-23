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

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ShowMessage("Введите логин и пароль");
                return;
            }

            var token = await _apiService.LoginAsync(username, password);
            if (token == null)
            {
                ShowMessage("Неверный логин или пароль");
                return;
            }

            Preferences.Set("auth_token", token);
            _apiService.SetToken(token);

            // Устанавливаем ApiService в статический класс
            AppState.ApiService = _apiService;

            // Переход на главную страницу
            if (DeviceInfo.Idiom == DeviceIdiom.Desktop || DeviceInfo.Idiom == DeviceIdiom.Tablet)
                Application.Current.MainPage = new NavigationPage(new DesktopMainPage(_apiService));
            else
                Application.Current.MainPage = new AppShell();
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

        private void ShowMessage(string text, bool isError = true)
        {
            MessageLabel.Text = text;
            MessageLabel.TextColor = isError ? Colors.Red : Colors.Green;
            MessageLabel.IsVisible = true;
        }
    }
}