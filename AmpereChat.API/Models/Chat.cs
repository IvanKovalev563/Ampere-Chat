namespace AmpereChat.API.Models
{
    public class Chat
    {
        public int chat_id { get; set; }
        public string name { get; set; } = "";
        public bool is_group { get; set; }
    }
}
