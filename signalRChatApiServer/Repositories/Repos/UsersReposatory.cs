using System.Linq;
using signalRChatApiServer.Data;
using System.Collections.Generic;
using signalRChatApiServer.Models;
using signalRChatApiServer.Repositories.Infra;

namespace signalRChatApiServer.Repositories.Repos
{
    public class UsersReposatory : IUsersRepository
    {
        private readonly TalkBackChatContext context;
        public UsersReposatory(TalkBackChatContext context)
        {
            this.context = context;
        }

        // when signing in / authenticating
        public User GetUser(int id) => context.Users.Find(id);

        //when fetching
        public List<User> GetAllUsers() => context.Users.ToList();

        public User Authenticate(string username, string password) => (from user in context.Users
                                                                       where user.UserName == username && password == user.Password
                                                                       select user).FirstOrDefault();

        public bool IsUserExist(string username) => context.Users.Where(u => u.UserName == username).Any();

        public int AddUser(User user)
        {
            var id = context.Users.Add(user).Entity.Id;
            context.SaveChanges();
            return id;
        }

        public void UpdateUser(User user)
        {
            var tempUser = context.Users.FirstOrDefault(u => u.Id == user.Id);
            if (tempUser == null) return;
            tempUser.Status = user.Status;
            tempUser.HubConnectionString = user.HubConnectionString;
            context.SaveChanges();
        }
    }
}
