using SocialNetwork_aspls17.Entities;

namespace SocialNetwork_aspls17.Models
{
    public class ChatViewModel
    {
        public string? CurrentUserId { get; set; }
        public Chat? CurrentChat { get; set; }
        //public IQueryable<Chat>? Chats { get; set; }
        public IEnumerable<Chat>? Chats { get; set; }
        public string CurrentReceiver { get; internal set; }
    }
}