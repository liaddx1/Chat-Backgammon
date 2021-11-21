using System.Collections.Generic;

namespace signalRChatApiServer.Models
{
    public class User
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public Status Status { get; set; }
        public string HubConnectionString { get; set; }

        public ICollection<Chat> Chats { get; set; }
        public List<ChatUser> ChatUsers { get; set; }
    }
}
