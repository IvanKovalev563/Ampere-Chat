using Ampere_Chat_Client.Services;
using Microsoft.Maui.Storage;

namespace Ampere_Chat_Client
{
    public partial class SettingsPage : ContentPage
    {
        public SettingsPage()
        {
            InitializeComponent();
        }

        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            Preferences.Remove("auth_token");
            AppState.ApiService = null;
            Application.Current.MainPage = new NavigationPage(new LoginPage(new ApiService()));
        }
    }
}