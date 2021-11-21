using signalRChatApiServer.Models;
using System.Collections.Generic;

namespace signalRChatApiServer.Repositories.Infra
{
    public interface IUsersRepository
    {
        int AddUser(User user);
        List<User> GetAllUsers();
        void UpdateUser(User user);
        User Authenticate(string username, string password);
        bool IsUserExist(string username);
        User GetUser(int userId);
    }
}