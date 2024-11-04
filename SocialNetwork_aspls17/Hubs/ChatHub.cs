using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SocialNetwork_aspls17.Data;
using SocialNetwork_aspls17.Entities;

namespace SocialNetwork_aspls17.Hubs
{
    public class ChatHub : Hub
    {
        private readonly UserManager<CustomIdentityUser>? _userManager;
        private readonly IHttpContextAccessor? _httpContextAccessor;
        private readonly SocialNetworkDbContext? _context;
        public ChatHub(UserManager<CustomIdentityUser>? userManager, IHttpContextAccessor? httpContextAccessor, 
                        SocialNetworkDbContext? context)
        {
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
            _context = context;
        }

        public override async Task OnConnectedAsync()
        {
            var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
            var userItem = _context.Users.SingleOrDefault(u => u.Id == user.Id);
            userItem.IsOnline = true;
            await _context.SaveChangesAsync();

            string info = user.UserName + " connected successfully !";
            await Clients.Others.SendAsync("Connect", info);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var user =await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
            var userItem = _context.Users.SingleOrDefault(u => u.Id == user.Id);
            userItem.IsOnline = false;
            await _context.SaveChangesAsync();

            string info = user.UserName + " disconnected !";
            await Clients.Others.SendAsync("Disconnect", info);
        }

        public async Task SendFollow(string id)
        {
            await Clients.User(id).SendAsync("ReceiveNotification");
        }

        public async Task CallOnlyMyRequests(string id)
        {
            await Clients.User(id).SendAsync("GetOnlyMyRequests");
        }


        public async Task CallOnlyAllUsers(string id)
        {
            await Clients.User(id).SendAsync("GetOnlyAllUsers");
        }


        public async Task GetMessages(string receiverId,string senderId)
        {
            await Clients.Users(new String[] { receiverId, senderId }).SendAsync("ReceiveMessages", receiverId, senderId);
        }
    }
}
