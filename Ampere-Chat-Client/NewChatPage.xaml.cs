using Ampere_Chat_Client.Services;
using System.Collections.ObjectModel;

namespace Ampere_Chat_Client
{
    public partial class NewChatPage : ContentPage
    {
        private readonly ApiService _apiService;
        private List<UserDto> _allUsers = new();
        public ObservableCollection<UserDto> FilteredUsers { get; set; } = new();

        public NewChatPage(ApiService apiService)
        {
            InitializeComponent();
            _apiService = apiService;
            BindingContext = this;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadUsers();
        }

        private async Task LoadUsers()
        {
            try
            {
                _allUsers = await _apiService.GetAllUsersAsync();
                // Исключаем текущего пользователя (его не должно быть в списке)
                var currentUserId = AppState.ApiService?.GetCurrentUserId() ?? 0;
                _allUsers = _allUsers.Where(u => u.user_id != currentUserId).ToList();

                System.Diagnostics.Debug.WriteLine($"Загружено пользователей: {_allUsers.Count}");
                foreach (var u in _allUsers)
                    System.Diagnostics.Debug.WriteLine($"  {u.user_id}: {u.username}");

                FilteredUsers.Clear();
                foreach (var user in _allUsers)
                    FilteredUsers.Add(user);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Не удалось загрузить пользователей: {ex.Message}", "ОК");
            }
        }

        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            var searchText = e.NewTextValue?.ToLower() ?? "";
            var filtered = _allUsers.Where(u => u.username.ToLower().Contains(searchText)).ToList();

            System.Diagnostics.Debug.WriteLine($"Поиск: '{searchText}', найдено: {filtered.Count}");
            foreach (var u in filtered)
                System.Diagnostics.Debug.WriteLine($"  {u.username}");

            FilteredUsers.Clear();
            foreach (var user in filtered)
                FilteredUsers.Add(user);

            UsersCollectionView.ItemsSource = null;
            UsersCollectionView.ItemsSource = FilteredUsers;
        }

        private async void OnUserSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is UserDto selectedUser)
            {
                UsersCollectionView.SelectedItem = null;

                try
                {
                    var result = await _apiService.CreatePrivateChatAsync(selectedUser.user_id);
                    if (result != null && result.chatId > 0)
                    {
                        // Закрываем текущую страницу и открываем чат
                        await Navigation.PopAsync();
                        await Navigation.PushAsync(new ChatPage(_apiService, result.chatId, selectedUser.username));
                    }
                    else
                    {
                        await DisplayAlert("Ошибка", "Не удалось создать чат", "ОК");
                    }
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Ошибка", ex.Message, "ОК");
                }
            }
        }
    }
}