using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using signalRChatApiServer.Hubs;
using signalRChatApiServer.Models;
using signalRChatApiServer.Repositories.Infra;

namespace signalRChatApiServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ChatController : Controller
    {
        private readonly IHubContext<ChatHub> chatHub;
        private readonly IChatsRepository repository;
        public ChatController(IChatsRepository repository, IHubContext<ChatHub> chatHub)
        {
            this.chatHub = chatHub;
            this.repository = repository;
        }

        [HttpGet]
        public void Get(int user1Id, int user2Id)
        {
            repository.IsChatExist(user1Id, user2Id, out Chat obj);

            foreach (var user in obj.Users)
            {
                user.Chats = null; user.ChatUsers = null;
            }
            obj.ChatUsers = null;

            foreach (var contact in obj.Users)
                chatHub.Clients.Client(contact.HubConnectionString).SendAsync("ChatCreated", obj);
        }

        [HttpPost]
        public void Post(Chat chat) => repository.AddChat(chat);

        [HttpPut]
        public void Put(Chat chat) => repository.UpdateChat(chat);
    }
}
