using Ampere_Chat_Client.Services;

namespace Ampere_Chat_Client
{
    public partial class DesktopMainPage : ContentPage
    {
        private readonly ApiService _apiService;

        public DesktopMainPage(ApiService apiService)
        {
            InitializeComponent();
            _apiService = apiService;

            // Подписываемся на событие выбора чата
            ChatsListView.ChatSelected += OnChatSelected;
            ChatsListView.LoadChats();
        }

        private void OnChatSelected(object sender, ChatDto selectedChat)
        {
            var chatDetail = new ChatDetailView(_apiService, selectedChat.chat_id);
            chatDetail.LoadMessages();
            ChatDetailContainer.Content = chatDetail;
        }

        private async void OnNewChatClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new NewChatPage(_apiService));
        }
    }
}