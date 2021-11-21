using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using signalRChatApiServer.Hubs;
using signalRChatApiServer.Models;
using signalRChatApiServer.Repositories.Infra;
using System.Collections.Generic;
using System.Linq;

namespace signalRChatApiServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MessagesController : Controller
    {
        private readonly IHubContext<ChatHub> chathub;
        readonly IMassegesReposatory repository;
        readonly IChatsRepository chatrepository;
        public MessagesController(IMassegesReposatory repository, IHubContext<ChatHub> chathub, IChatsRepository chatrepository)
        {
            this.chathub = chathub;
            this.chatrepository = chatrepository;
            this.repository = repository;
        }

        [HttpGet]
        public List<Message> Get(int chatId)
        {
            return repository.GetMessages(chatId).ToList() ?? new List<Message>();
        }

        //Put
        [HttpPost]
        public void Post(Message message)
        {
            repository.AddMessage(message);
            var chat = chatrepository.GetChat(message.ChatId);

            foreach (var item in chat.Users)
            {
                chathub.Clients.Client(item.HubConnectionString).SendAsync("MassageRecived", message);
            }
        }

    }
}
