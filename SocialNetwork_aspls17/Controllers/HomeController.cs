using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialNetwork_aspls17.Data;
using SocialNetwork_aspls17.Entities;
using SocialNetwork_aspls17.Models;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;

namespace SocialNetwork_aspls17.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<CustomIdentityUser> _userManager;
        private readonly SocialNetworkDbContext _context;

        public HomeController(ILogger<HomeController> logger, UserManager<CustomIdentityUser> userManager, SocialNetworkDbContext context)
        {
            _logger = logger;
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            ViewBag.User = user;
            return View();
        }

        public async Task<IActionResult> GetAllUsers()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var myrequests = _context.FriendRequests.Where(r => r.SenderId == user.Id);

            var myfriends = _context.Friends.Where(f => f.OwnId == user.Id || f.YourFriendId == user.Id);

            var users = await _context.Users
                                      .Where(u => u.Id != user.Id)
                                      .OrderByDescending(u => u.IsOnline)
                                      .Select(u => new CustomIdentityUser
                                      {
                                          Id = u.Id,
                                          HasRequestPending = (myrequests.FirstOrDefault(r => r.ReceiverId == u.Id && r.Status == "Request") != null),
                                          UserName = u.UserName,
                                          IsOnline = u.IsOnline,
                                          Image = u.Image,
                                          Email = u.Email,
                                          IsFriend = (myfriends.FirstOrDefault(f => f.OwnId == u.Id || f.YourFriendId == u.Id) != null)
                                      })
                                      .ToListAsync();

            //var users = await _context.Users.Where(u => u.Id != user.Id)
            //                                .OrderByDescending(u => u.IsOnline)
            //                                .ToListAsync();
            //var myrequests = _context.FriendRequests.Where(r => r.SenderId == user.Id);
            //foreach (var item in users)
            //{
            //    var request = myrequests.FirstOrDefault(r => r.ReceiverId == item.Id && r.Status=="Request");
            //    if (request != null)
            //    {
            //        item.HasRequestPending = true;
            //    }
            //}

            return Ok(users);
        }

        public async Task<IActionResult> SendFollow(string id)
        {
            var sender = await _userManager.GetUserAsync(HttpContext.User);
            var receiverUser = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (receiverUser != null)
            {
                //receiverUser.FriendRequests.Add(new FriendRequest
                //{
                //});
                _context.FriendRequests.Add(new FriendRequest
                {
                    Content = $"{sender.UserName} sent friend request at {DateTime.Now.ToLongDateString()} to {receiverUser.UserName}",
                    SenderId = sender.Id,
                    Sender = sender,
                    ReceiverId = id,
                    Status = "Request"
                });
                await _context.SaveChangesAsync();
                //await _userManager.UpdateAsync(receiverUser);
                return Ok();
            }
            return BadRequest();
        }

        public async Task<IActionResult> GetAllRequests()
        {
            //Bu requesti alan sexsden gelen funksiyadir
            var current = await _userManager.GetUserAsync(HttpContext.User);
            var requests = _context.FriendRequests.Where(r => r.ReceiverId == current.Id);
            return Ok(requests);
        }


        public async Task<IActionResult> AcceptRequest(string receiverId, string senderId, int requestId)
        {
            var receiverUser = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == receiverId);
            var sender = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == senderId);
            if (receiverUser != null)
            {
                _context.FriendRequests.Add(new FriendRequest
                {
                    Content = $"{sender.UserName} accepted friend request from {receiverUser.UserName} at {DateTime.Now.ToLongDateString()} {DateTime.Now.ToShortTimeString()}",
                    SenderId = sender.Id,
                    Sender = sender,
                    ReceiverId = receiverId,
                    Status = "Notification"
                });
                var request = await _context.FriendRequests.FirstOrDefaultAsync(r => r.Id == requestId);
                _context.FriendRequests.Remove(request);

                _context.Friends.Add(new Friend
                {
                    OwnId = sender.Id,
                    YourFriendId = receiverUser.Id
                });

                await _context.SaveChangesAsync();
                return Ok();
            }
            return BadRequest();
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteRequest(int requestId, string senderId, string receiverId)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(HttpContext.User);
                var request = await _context.FriendRequests.FirstOrDefaultAsync(r => r.Id == requestId && r.ReceiverId == receiverId);
                //var request = await _context.FriendRequests.FirstOrDefaultAsync(r => r.Id == requestId && r.ReceiverId==currentUser.Id);
                if (request == null) return NotFound();
                _context.FriendRequests.Remove(request);
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        public async Task<IActionResult> DeclineRequest(int currentId, string senderId)
        {
            try
            {
                var senderUser = await _context.Users.FirstOrDefaultAsync(s => s.Id == senderId);
                var current = await _userManager.GetUserAsync(HttpContext.User);
                var request = await _context.FriendRequests.FirstOrDefaultAsync(r => r.ReceiverId == current.Id && r.SenderId == senderId);
                _context.FriendRequests.Remove(request);

                _context.FriendRequests.Add(new FriendRequest
                {
                    Content = $"{current.UserName} declined your {senderUser.UserName} friend request at {DateTime.Now.ToLongDateString()} {DateTime.Now.ToShortTimeString()}",
                    SenderId = current.Id,
                    Sender = current,
                    ReceiverId = senderId,
                    Status = "Notification"
                });
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        public async Task<IActionResult> TakeRequest(string receiverId)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(HttpContext.User);
                var request = await _context.FriendRequests.FirstOrDefaultAsync(r => r.ReceiverId == receiverId && r.SenderId == currentUser.Id);
                if (request == null) return NotFound("Request is null !");
                _context.FriendRequests.Remove(request);
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        public async Task<IActionResult> UnFollowFriend(string friendId)
        {

            var currentUser = await _userManager.GetUserAsync(HttpContext.User);
            var friend = await _context.Friends
                        .FirstOrDefaultAsync(f => f.YourFriendId == currentUser.Id && f.OwnId == friendId
                                                 || f.YourFriendId == friendId && f.OwnId == currentUser.Id);

            var otherUser = _context.Users.FirstOrDefault(u => u.Id == friendId);
            if (friend == null) return NotFound("Friend is null");

            _context.FriendRequests.Add(new FriendRequest
            {
                Content = $"{currentUser.UserName} unfollowed friend from {otherUser.UserName} at {DateTime.Now.ToLongDateString()} {DateTime.Now.ToShortTimeString()}",
                SenderId = currentUser.Id,
                Sender = currentUser,
                ReceiverId = friendId,
                Status = "Notification"
            });

            _context.Friends.Remove(friend);
            await _context.SaveChangesAsync();
            return Ok();
        }



        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        public async Task<IActionResult> GoChat(string id)
        {
            var isSender = false;
            var currentUser = await _userManager.GetUserAsync(HttpContext.User);
            var chat = await _context.Chats.Include(nameof(Chat.Messages)).FirstOrDefaultAsync(ch => ch.SenderId == currentUser.Id && ch.ReceiverId == id
                                                            || ch.ReceiverId == currentUser.Id && ch.SenderId == id);

            if (chat == null)
            {
                chat = new Chat
                {
                    ReceiverId = id,
                    SenderId = currentUser.Id,
                    Messages = new List<Message>()
                };
                await _context.Chats.AddAsync(chat);
                await _context.SaveChangesAsync();
            }

            var chats = _context.Chats.Include(nameof(Chat.Receiver)).Where(ch => ch.SenderId == currentUser.Id || ch.ReceiverId == currentUser.Id);

            var chatBlocks = chats.Select(c => new Chat
            {
                Messages = c.Messages,
                Id = c.Id,
                ReceiverId = c.ReceiverId != currentUser.Id ? c.ReceiverId : c.SenderId,
                SenderId = c.SenderId != currentUser.Id ? c.ReceiverId : c.SenderId,
                Receiver = c.ReceiverId != currentUser.Id ? c.Receiver : _context.Users.FirstOrDefault(u => u.Id == c.SenderId),

            });
            //var chatBlocks = chats.Select(c => new Chat//Burda duz islemir,cunki eger dostu ilk send mesaja kliklese
            //onda bu defe ReseiverId-ozu olur,ona gorede ozu ile chat yaradir ve buda bug demekdir
            //{
            //    Messages = c.Messages,
            //    Id = c.Id,
            //    ReceiverId = c.ReceiverId,
            //    SenderId =c.SenderId,
            //    Receiver = (c.ReceiverId != currentUser.Id) ? c.Receiver : _context.Users.FirstOrDefault(u => u.Id == c.SenderId)
            //});

            //var chatBlocks = from c in chats
            //                 let receiver = (c.ReceiverId != currentUser.Id) ? c.Receiver : _context.Users.FirstOrDefault(u => u.Id == c.SenderId)
            //                 select new Chat
            //                 {
            //                     Messages = c.Messages,
            //                     Id = c.Id,
            //                     SenderId = c.SenderId,
            //                     ReceiverId = c.ReceiverId,
            //                     //Messages = c.Messages,
            //                     //Id = c.Id,
            //                     //ReceiverId = c.ReceiverId != currentUser.Id ? c.ReceiverId : c.SenderId,
            //                     //SenderId = c.SenderId != currentUser.Id ? c.ReceiverId : c.SenderId,
            //                     Receiver = receiver
            //                 };

            //var result = chatBlocks.ToList().Where(c => c.ReceiverId != currentUser.Id);


            var model = new ChatViewModel
            {
                CurrentUserId = currentUser.Id,
                CurrentChat = chat,
                //Chats = result
                Chats = chatBlocks,
                CurrentReceiver = id
            };
            return View(model);
        }


        [HttpPost(Name = "AddMessage")]
        public async Task<IActionResult> AddMessage(MessageModel model)
        {
            try
            {
                var currentChat = _context.Chats?.FirstOrDefault(c => c.SenderId == model.SenderId && c.ReceiverId == model.ReceiverId
                                                                        || c.SenderId == model.ReceiverId && c.ReceiverId == model.SenderId);

                if (currentChat != null)
                {
                    var message = new Message
                    {
                        ChatId = currentChat.Id,
                        Content = model.Content,
                        DateTime = DateTime.Now,
                        IsImage = false,
                        HasSeen = false
                    };
                    await _context.Messages.AddAsync(message);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok();
        }

        public async Task<IActionResult> GetAllMessages(string receiverId,string senderId)
        {
            var currentChat = _context.Chats?.Include(nameof(Chat.Messages)).FirstOrDefault(c => c.SenderId ==senderId && c.ReceiverId == receiverId
                                                                      || c.SenderId == receiverId && c.ReceiverId == senderId);
            if (currentChat != null)
            {
                var currentUser =await _userManager.GetUserAsync(HttpContext.User);
                return Ok(new { Messages = currentChat.Messages, CurrentUserId = currentUser.Id });
            }
            return Ok("Chat is null !");
        }







    }
}
