using Ampere_Chat_Client.Services;
using System.Collections.ObjectModel;

namespace Ampere_Chat_Client
{
    public partial class ChatsView : ContentView
    {
        private ApiService _apiService => AppState.ApiService;
        public ObservableCollection<ChatDto> Chats { get; set; } = new();

        public event EventHandler<ChatDto> ChatSelected;

        public ChatsView()
        {
            InitializeComponent();
            BindingContext = this;
        }

        protected override void OnParentSet()
        {
            base.OnParentSet();
            if (Parent != null)
                LoadChats();
        }
        public async void LoadChats()
        {
            try
            {
                if (_apiService == null) return;
                var chatList = await _apiService.GetChatsAsync();
                Chats.Clear();
                foreach (var chat in chatList)
                    Chats.Add(chat);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки чатов: {ex.Message}");
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