namespace AmpereChat.API.Models
{
    public class Message
    {
        public int message_id { get; set; }
        public int chat_id { get; set; }
        public int user_id { get; set; }
        public string text { get; set; } = "";
        public DateTime sent_at { get; set; }
    }
}
