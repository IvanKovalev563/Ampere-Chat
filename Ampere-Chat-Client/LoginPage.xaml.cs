using Ampere_Chat_Client.Services;
using Microsoft.Maui.Storage;

namespace Ampere_Chat_Client
{
    public partial class LoginPage : ContentPage
    {
        private readonly ApiService _apiService;

        public LoginPage()
        {
            InitializeComponent();
            _apiService = new ApiService();
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

            // Сохраняем токен
            await SecureStorage.SetAsync("auth_token", token);
            _apiService.SetToken(token);

            // Переход на главную страницу
            await Navigation.PushAsync(new ChatsPage(_apiService));
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