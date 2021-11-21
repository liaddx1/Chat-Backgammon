using System.Linq;
using signalRChatApiServer.Data;
using System.Collections.Generic;
using signalRChatApiServer.Models;
using signalRChatApiServer.Repositories.Infra;

namespace signalRChatApiServer.Repositories.Repos
{
    public class ChatsReposatory : IChatsRepository
    {
        private readonly TalkBackChatContext context;

        public ChatsReposatory(TalkBackChatContext context) => this.context = context;

        public Chat GetChat(int id)
        {
            return (from chat in context.Chats
                    where chat.Id == id
                    select new Chat
                    {
                        Id = id,
                        InviteStatus = chat.InviteStatus,
                        Users = chat.Users,
                        Messages = chat.Messages,
                        ChatUsers = chat.ChatUsers
                    }).FirstOrDefault();
        }

        //when openning a room
        public int AddChat(Chat chat) 
        {
            if (chat == null || chat.Users.Contains(null)) return 0;
            var e = context.Chats.Add(chat);
            context.SaveChanges();
            return e.Entity.Id;
        }

        public Chat CreateChatWithUser(int userId, int toUser)
        {
            if (context.Users.ToArray().Length < 2 ||
                userId == toUser ||
                context.Users.Find(userId) == null ||
                context.Users.Find(toUser) == null)
                return null;
            var isExist = from chat in context.Chats
                          where chat.Users.Where(u => u.Id == userId).Any()
                             && chat.Users.Where(u => u.Id == userId).Any()
                          select chat;
            if (isExist.Any()) return isExist.FirstOrDefault();
            var userA = context.Users.Find(userId);
            var userB = context.Users.Find(toUser);
            var newChat = new Chat { Users = new List<User> { userA, userB } };
            AddChat(newChat);
            return newChat;
        }

        public bool IsChatExist(int user1Id, int user2Id, out Chat c)
        {
            var qchat = (from chat in context.Chats
                             //join user in context.Users
                             //on chat.ChatUsers equals user.ChatUsers
                         where chat.Users.Contains(context.Users.Find(user2Id)) &&
                                chat.Users.Contains(context.Users.Find(user1Id))
                         select new Chat { Id = chat.Id, Users = chat.Users, Messages = chat.Messages, }).Take(1);
            if (qchat.Any())
            {
                c = qchat.First();
                //c.Users = qchat.Users;
                return true;
            }
            else
            {
                c = CreateNewChat(user1Id, user2Id); return false;
            }
        }

        public void UpdateChat(Chat chat)
        {
            var tempChat = context.Chats.Where(c => c.Id == chat.Id).FirstOrDefault();
            if (tempChat == null) return;
            tempChat.Messages = chat.Messages;
            //tempChat.Users = chat.Users;
            tempChat.Id = chat.Id;
            //tempChat.ChatUsers = chat.ChatUsers;
            tempChat.InviteStatus = chat.InviteStatus;
            context.SaveChanges();
        }

        public Chat CreateNewChat(int user1Id, int user2Id)
        {
            var chat = new Chat { Users = new List<User> { context.Users.Find(user1Id), context.Users.Find(user2Id) }, Messages = new List<Message>() };
            AddChat(chat);
            return chat;
        }
    }
}
