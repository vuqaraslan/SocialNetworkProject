﻿namespace SocialNetwork_aspls17.Entities
{
    public class Message
    {
        public int Id { get; set; }
        public string? Content { get; set; }
        public bool IsImage { get; set; }
        public DateTime DateTime { get; set; }
        public int ChatId { get; set; }
        public virtual Chat? Chat { get; set; }
        public bool HasSeen { get; set; }
    }
}
