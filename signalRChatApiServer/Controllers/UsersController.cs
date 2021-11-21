using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using signalRChatApiServer.Hubs;
using signalRChatApiServer.Models;
using signalRChatApiServer.Repositories.Infra;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace signalRChatApiServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsersController : Controller
    {
        private IHubContext<ChatHub> chathub;
        private IUsersRepository repository;

        public UsersController(IUsersRepository repository, IHubContext<ChatHub> chatHub)
        {
            chathub = chatHub;
            this.repository = repository;
        }

        [HttpGet]
        public List<User> Get()
        {
            return repository.GetAllUsers();
        }
        [HttpPut]
        public void Put(User user)
        {
            if (user.Status == Status.Offline)
            {
                user.HubConnectionString = "";
                chathub.Clients.AllExcept(user.HubConnectionString).SendAsync("ContactLoggedOut", user);
                chathub.Clients.Client(user.HubConnectionString).SendAsync("LoggingOut", user);
            }
            repository.UpdateUser(user);
        }

    }
}
