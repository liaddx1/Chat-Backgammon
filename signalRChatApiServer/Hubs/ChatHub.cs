using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace signalRChatApiServer.Hubs
{
    public class ChatHub : Hub
    {
        public override Task OnConnectedAsync()
        {
            Clients.Caller.SendAsync("Connected", Context.ConnectionId);

            return base.OnConnectedAsync();
        }
    }
}
