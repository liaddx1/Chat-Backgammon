using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using signalRChatApiServer.Hubs;
using signalRChatApiServer.Models;
using signalRChatApiServer.Repositories.Infra;
using System.Linq;
using System.Threading.Tasks;

namespace signalRChatApiServer.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class GameController : ControllerBase
    {
        private IChatsRepository chatRepository;
        private IUsersRepository usersRepository;
        private IHubContext<ChatHub> chatHub;

        public GameController(IChatsRepository chatRepository, IHubContext<ChatHub> chatHub, IUsersRepository usersRepository)
        {
            this.usersRepository = usersRepository;
            this.chatRepository = chatRepository;
            this.chatHub = chatHub;
        }
        [HttpPost]
        public async Task Post(ActionUpdateModel obj)
        {
            var chat = chatRepository.GetChat(obj.ChatId);
            obj.InverseRows();
            //send to other user
            var user = chat.Users.First(u => u.Id != obj.UserId);
            await chatHub.Clients.Client(user.HubConnectionString).SendAsync("OpponentPlayed", obj);
        }

        [HttpGet]
        public async Task Get(int userId)
        {
            User user = usersRepository.GetUser(userId);
            await chatHub.Clients.Client(user.HubConnectionString).SendAsync("PlayerFinnishedPlay");
        }

        [HttpGet]
        [Route("GameOver")]
        public void AnnounceWinner(int userId,int chatId)
        {
            var chat = chatRepository.GetChat(chatId);
            var user = chat.Users.First(u => u.Id != userId);
            chatHub.Clients.Client(user.HubConnectionString).SendAsync("GameOver");
            foreach (var u in chat.Users)
            {
                user.Status = Status.Online;
                usersRepository.UpdateUser(user);
            }
        }

        [HttpGet]
        [Route("Forfeit")]
        public async Task Get(string chatId)
        {
            Chat chat = chatRepository.GetChat(int.Parse(chatId));
              
            foreach (var user in chat.Users)
            {
                user.Status = Status.Online;
                usersRepository.UpdateUser(user);
                await chatHub.Clients.Client(user.HubConnectionString).SendAsync("GameEnded");
            }
        }
    }
}
