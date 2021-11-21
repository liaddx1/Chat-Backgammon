using System.Linq;
using System.Net.Http;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using tWpfMashUp_v0._0._1.MVVM.Models;
using tWpfMashUp_v0._0._1.Assets.Components.CustomModal;

namespace tWpfMashUp_v0._0._1.Sevices
{
    public class ChatsService
    {
        private readonly StoreService store;
        public ChatsService(StoreService store) => this.store = store;

        public async Task<Chat> GetChatAsync(int userToId)
        {
            var contacts = store.Get(CommonKeys.Contacts.ToString()) as List<User>;
            if (contacts != null && contacts.Where(u => u.Id == userToId).Any()) return null;
            var id = ((User)store.Get(CommonKeys.LoggedUser.ToString())).Id;
            var url = @$"http://localhost:14795/Chat?userId={id}&toUserId={userToId} ";
            Chat chat;
            using (HttpClient client = new())
            {
                try
                {
                    var response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    var resString = await response.Content.ReadAsStringAsync();
                    chat = JsonConvert.DeserializeObject<Chat>(resString);
                }
                catch { Modal.ShowModal("Failed To Get Chat"); return null; }
            }
            if (chat == null)
            {
                Modal.ShowModal("Cannot Create Chat, Chat Already Exists!");
                return null;
            }

            return HandleNewChatForStore(contacts, id, chat);
        }

        private Chat HandleNewChatForStore(List<User> contacts, int id, Chat chat)
        {
            var contact = chat.Users.Where(u => u.Id != id).First();

            if (contacts == null) contacts = new List<User>();
            contacts.Add(contact);
            store.Add(CommonKeys.Contacts.ToString(), contacts);
            if (store.Get(CommonKeys.Chats.ToString()) is not List<Chat> chats) chats = new List<Chat>();
            chats.Add(chat);
            store.Add(CommonKeys.Chats.ToString(), chat);
            return chat;
        }

        public async Task CreateChatIfNotExistAsync(User newCurrentUser)
        {
            if (store.HasKey(CommonKeys.Chats.ToString()))
            {
                var chats = store.Get(CommonKeys.Chats.ToString()) as List<Chat>;
                var chatToReturn = chats.Find(c => c.Users.Where(u => u.Id == newCurrentUser.Id).Any());
                if (chatToReturn != null)
                {
                    store.Add(CommonKeys.CurrentChat.ToString(), chatToReturn);
                    var newContactName = (store.Get(CommonKeys.WithUser.ToString()) as User).UserName;
                    store.InformContactChanged(this, new ChatRecivedEventArgs { NewChat = chatToReturn, ContactName = newContactName });
                    return;
                }
            }
            var loggedUser = store.Get(CommonKeys.LoggedUser.ToString()) as User;
            var url = @$"http://localhost:14795/Chat?user1Id={loggedUser.Id}&user2Id={newCurrentUser.Id}";
            try
            {
                using (HttpClient client = new())
                {
                    var res = await client.GetAsync(url);
                    res.EnsureSuccessStatusCode();
                }
            }
            catch { }
        }
    }
}