using signalRChatApiServer.Models;
using System.Collections.Generic;

namespace signalRChatApiServer.Repositories.Infra
{
    public interface IMassegesReposatory
    {
        bool AddMessage(Message message);
        IEnumerable<Message> GetMessages(int chatId);
    }
}