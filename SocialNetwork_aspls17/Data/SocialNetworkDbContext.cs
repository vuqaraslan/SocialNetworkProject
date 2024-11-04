using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SocialNetwork_aspls17.Entities;

namespace SocialNetwork_aspls17.Data
{
    public class SocialNetworkDbContext : IdentityDbContext<CustomIdentityUser, CustomIdentityRole, string>
    {
        public SocialNetworkDbContext(DbContextOptions<SocialNetworkDbContext> options)
            : base(options)
        { }
        public DbSet<Friend>? Friends { get; set; }
        public DbSet<FriendRequest>? FriendRequests { get; set; }
        public DbSet<Chat>? Chats { get; set; }
        public DbSet<Message>? Messages { get; set; }
    }
}
