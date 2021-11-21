using System;
using System.Text;
using System.Net.Http;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using tWpfMashUp_v0._0._1.Extentions;
using tWpfMashUp_v0._0._1.MVVM.Models;
using tWpfMashUp_v0._0._1.Assets.Components.CustomModal;

namespace tWpfMashUp_v0._0._1.Sevices
{
    public class AuthenticationService
    {
        private readonly StoreService storeService;
        public event EventHandler LoggingIn;
        public event EventHandler LoggingOut;

        public AuthenticationService(StoreService storeService)
        {
            this.storeService = storeService;
            App.Current.Exit += async (s, e) => await InvokeLogOut();
        }
        public async Task<bool> CallServerToSignUp(string username, string password)
        {
            if (!username.IsEmptyNullOrWhiteSpace() && !password.IsEmptyNullOrWhiteSpace())
            {
                if (username.Length > 2 && password.Length > 2)
                {
                    var url = @"http://localhost:14795/Authentication";
                    using HttpClient client = new();
                    try
                    {
                        var values = new User { UserName = username, Password = password };
                        var content = new StringContent(JsonConvert.SerializeObject(values), Encoding.UTF8, "application/json");
                        var response = await client.PostAsync(url, content);
                        var rawData = await response.Content.ReadAsStringAsync();
                        if (rawData == "false")
                        {
                            Modal.ShowModal("User already exists!");
                            return false;
                        }
                        return true;
                    }
                    catch (Exception ex) { Modal.ShowModal(ex.Message, "Failed to call server"); }
                    return false;
                }
                Modal.ShowModal("Username and Password must be at lease 2 characters!");
                return false;
            }
            Modal.ShowModal("Username and Password cannot be empty!");
            return false;
        }

        public async Task<bool> LoginAsync(string username, string password)
        {

            var hubstring = storeService.Get(CommonKeys.HubConnectionString.ToString()) as string;
            var url = @$"http://localhost:14795/Authentication?username={username}&password={password}&hubstring={hubstring}";
            using HttpClient client = new();

            try
            {
                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                var rawData = await response.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<User>(rawData);
                if (data != null)
                {
                    storeService.Add(CommonKeys.LoggedUser.ToString(), data);
                    if (data.Chats != null)
                        storeService.Add(CommonKeys.Chats.ToString(), data.Chats);

                    await FetchAllLoggedUsers();
                    LoggingIn?.Invoke(this, new EventArgs());
                    return true;
                }
            }
            catch { Modal.ShowModal("Failed To Call Server"); }
            return false;
        }

        public async Task InvokeLogOut()
        {
            await UpdateServerUserLogOut();
            LoggingOut?.Invoke(this, new EventArgs());
            storeService.OnLogOut();
        }

        public async Task UpdateServerUserLogOut()
        {
            if (storeService.Get(CommonKeys.LoggedUser.ToString()) is not User loggedUser) return;
            loggedUser.Status = Status.Offline;
            var url = @$"http://localhost:14795/Users";
            using HttpClient client = new();
            try
            {
                var content = new StringContent(JsonConvert.SerializeObject(loggedUser), Encoding.UTF8, "application/json");
                var response = await client.PutAsync(url, content);
                response.EnsureSuccessStatusCode();
            }
            catch { }
        }

        public async Task FetchAllLoggedUsers()
        {
            var url = @$"http://localhost:14795/Users";
            using HttpClient client = new();
            try
            {
                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                var rawData = await response.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<List<User>>(rawData);
                if (data != null)
                {
                    var me = storeService.Get(CommonKeys.LoggedUser.ToString()) as User;
                    data.Remove(data.Find(u => u.Id == me.Id));
                    storeService.Add(CommonKeys.Contacts.ToString(), data);
                }
            }
            catch { Modal.ShowModal("Failed To Call Server"); }
        }
    }
}
