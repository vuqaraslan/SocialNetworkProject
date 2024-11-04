﻿namespace SocialNetwork_aspls17.Entities
{
    public class FriendRequest
    {
        public int Id { get; set; }
        public string? Content { get; set; }
        public string? Status { get; set; }
        public string? SenderId { get; set; }
        public virtual CustomIdentityUser? Sender { get; set; }
        public string? ReceiverId { get; set; }
    }
}
