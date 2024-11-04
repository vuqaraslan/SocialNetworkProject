using System.Reflection.Metadata.Ecma335;

namespace SocialNetwork_aspls17.Models
{
    public class MessageModel
    {
        public string? SenderId { get; set; }
        public string? ReceiverId { get; set; }
        public string? Content { get; set; }
    }
}