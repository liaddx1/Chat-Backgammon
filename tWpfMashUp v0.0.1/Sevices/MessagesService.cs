using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using tWpfMashUp_v0._0._1.Assets.Components.CustomModal;
using tWpfMashUp_v0._0._1.MVVM.Models;

namespace tWpfMashUp_v0._0._1.Sevices
{
    public class MessagesService
    {
        private readonly StoreService storeService;
        public MessagesService(StoreService storeService)
        {
            this.storeService = storeService;
        }
        public async Task<bool> CallServerToAddMessage(string message)
        {
            var url = @"http://localhost:14795/Messages";
            using HttpClient client = new();
            try
            {
                if (storeService.Get(CommonKeys.CurrentChat.ToString()) is not Chat chat) { Modal.ShowModal("No Chat Selected for messages"); return false; }
                var msg = new Message
                {
                    Content = message,
                    Date = DateTime.Now,
                    Name = ((User)storeService.Get(CommonKeys.LoggedUser.ToString())).UserName,
                    ChatId = chat.Id
                };

                var content = new StringContent(JsonConvert.SerializeObject(msg), Encoding.UTF8, "application/json");
                var response = await client.PostAsync(url, content);
                response.EnsureSuccessStatusCode();
                return true;
            }
            catch (Exception ex) { Modal.ShowModal(ex.Message, "Failed to call server"); return false; }
        }

        public async Task<List<Message>> GetChatMassages(int chatId)
        {
            var url = @$"http://localhost:14795/Messages?chatId={chatId}";
            using HttpClient client = new();
            try
            {
                var response = await client.GetAsync(url);
                var readData = await response.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<List<Message>>(readData);
                return data;
            }
            catch (Exception ex)
            {
                Modal.ShowModal(ex.Message, "Failed to call server");
                return null;
            }
        }
    }
}
