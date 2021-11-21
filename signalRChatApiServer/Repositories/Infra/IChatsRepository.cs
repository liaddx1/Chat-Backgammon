using signalRChatApiServer.Models;

namespace signalRChatApiServer.Repositories.Infra
{
    public interface IChatsRepository
    {
        int AddChat(Chat chat);
        Chat CreateChatWithUser(int userId, int toUser);
        Chat CreateNewChat(int user1Id, int user2Id);
        Chat GetChat(int id);
        bool IsChatExist(int user1Id, int user2Id, out Chat c);
        void UpdateChat(Chat chat);
    }
}