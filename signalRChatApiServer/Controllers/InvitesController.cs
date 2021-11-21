using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using signalRChatApiServer.Hubs;
using signalRChatApiServer.Models;
using signalRChatApiServer.Repositories.Infra;
using System;
using System.Linq;

namespace signalRChatApiServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class InvitesController : Controller
    {
        private IChatsRepository reposatory;
        private IUsersRepository userReposatory;
        private IHubContext<ChatHub> chathub;

        public InvitesController(IHubContext<ChatHub> chathub, IChatsRepository reposatory, IUsersRepository usersRepository)
        {
            this.reposatory = reposatory;
            this.userReposatory = usersRepository;
            this.chathub = chathub;
        }

        [HttpPut]
        public void Put(Chat chat)
        {
            foreach (var user in chat.Users)
            {
                if (user.Status == Status.InGame)
                {
                    var calleruser = chat.Users.First(u => u.Id != user.Id);
                    chathub.Clients.Client(calleruser.HubConnectionString)
                    .SendAsync("GameDenied",$"{user.UserName} Is currently in a game");
                    return;
                }
            }
            foreach (var user in chat.Users)
            {
                chathub.Clients.Client(user.HubConnectionString).SendAsync("GameInvite", chat);
            }
        }

        [HttpGet]
        public void Get(int chatId, bool accepted)
        {
            var chat = reposatory.GetChat(chatId);
            if (accepted)
            {
                chat.InviteStatus = (InviteStatus)((int)chat.InviteStatus + 1);
                reposatory.UpdateChat(chat);

                if (chat.InviteStatus == InviteStatus.Accepted)
                {
                    var rnd = new Random().Next(0, 2);
                    var temp = 0;
                    foreach (var user in chat.Users)
                    {
                        user.Status = Status.InGame;
                        userReposatory.UpdateUser(user);
                        chathub.Clients.Client(user.HubConnectionString).SendAsync("GameStarting", chat.Id, temp == rnd);
                        temp++;
                    }
                    chat.InviteStatus = InviteStatus.Empty;
                    reposatory.UpdateChat(chat);
                }
            }
            else
            {
                chat.InviteStatus = InviteStatus.Empty;
                reposatory.UpdateChat(chat);
                foreach (var user in chat.Users)
                {
                    try
                    {
                        chathub.Clients.Client(user.HubConnectionString).SendAsync("GameDenied", "Game request was denied");
                    }
                    catch { }
                }
            }
        }
    }
}
