using System;

namespace signalRChatApiServer.Models
{
    public class Message
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public DateTime Date { get; set; }
        public string Name { get; set; }
        public int ChatId { get; set; }
    }
}