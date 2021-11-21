using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using signalRChatApiServer.Hubs;
using signalRChatApiServer.Models;
using signalRChatApiServer.Repositories.Infra;

namespace signalRChatApiServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthenticationController : Controller
    {
        private readonly IHubContext<ChatHub> chatHub;
        private readonly IUsersRepository repository;

        public AuthenticationController(IUsersRepository repository, IHubContext<ChatHub> chatHub)
        {
            this.chatHub = chatHub;
            this.repository = repository;
        }

        //For login
        [HttpGet]
        public User Get(string username, string password, string hubstring)
        {
            var isauth = repository.Authenticate(username, password);
            if (isauth != null && isauth.Status != Status.Online && isauth.Status != Status.InGame)
            {
                isauth.HubConnectionString = hubstring;
                isauth.Status = Status.Online;
                repository.UpdateUser(isauth);
                chatHub.Clients.AllExcept(isauth.HubConnectionString).SendAsync("ContactLoggedIn", isauth);
                return isauth;
            }
            return null;
        }

        //for sign up
        [HttpPost]
        public bool Post(User newUser)
        {
            try
            {
                var isExist = repository.IsUserExist(newUser.UserName);
                if (!isExist)
                {
                    repository.AddUser(newUser);
                    return true;
                }
                return false;
            }
            catch { return false; }
        }
    }
}
