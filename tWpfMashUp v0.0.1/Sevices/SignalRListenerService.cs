using System;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using tWpfMashUp_v0._0._1.MVVM.Models;
using Microsoft.AspNetCore.SignalR.Client;
using tWpfMashUp_v0._0._1.MVVM.Models.GameModels;
using tWpfMashUp_v0._0._1.Assets.Components.CustomModal;

namespace tWpfMashUp_v0._0._1.Sevices
{ 
    public class SignalRListenerService
    {
        #region services
        private readonly StoreService store;
        private readonly MessagesService messagesService;
        private readonly HubConnection connection;
        #endregion

        #region events
        public event ContactLoggedEventHandler ContactLogged;
        public event EventHandler LoggingOut;
        public event EventHandler ChatForUserRecived;
        public event MessageRecivedEventHandler MessageRecived;
        public event UserInvitedEventHandler UserInvitedToGame;
        public event GameStartingEventHandler GameStarting;
        public event OpponentPlayedEventHandler OpponentPlayed;
        public event EventHandler OpponentFinnishedPlay;
        public event EventHandler GameEnded;
        #endregion

        public SignalRListenerService(StoreService store, MessagesService messagesService)
        {
            this.store = store;
            this.messagesService = messagesService;
            connection = new HubConnectionBuilder().WithUrl("http://localhost:14795/ChatHub").Build();
            connection.Closed += async (err) => { await Task.Delay(2500); await connection.StartAsync(); };
            StartConnectionAsync();
        }

        public async void StartConnectionAsync()
        {
            connection.On<string>("Connected", OnConnected);
            connection.On<User>("ContactLoggedIn", OnContactLoggedIn);
            connection.On<User>("ContactLoggedOut", OnContactLoggedOut);
            connection.On<User>("LoggingOut", OnLoggingOut);
            connection.On<Chat>("ChatCreated", OnChatCreated);
            connection.On<Message>("MassageRecived", OnMassageRecived);

            connection.On<Chat>("GameInvite", OnGameInvite);
            connection.On<int,bool>("GameStarting", OnGameAccepted);
            connection.On<string>("GameDenied", OnGameDenied);

            connection.On<ActionUpdateModel>("OpponentPlayed", OnPlayerPlayed);
            connection.On("PlayerFinnishedPlay", OnPlayerFinnishedPlay);
            connection.On("GameOver", OnGameOver);
            connection.On("GameEnded", OnPlayerForfeit);

            try
            {
                if (connection.State != HubConnectionState.Connected) await connection.StartAsync();
            }
            catch (Exception ex) { Debug.WriteLine(ex.Message); }
        }

        private void OnLoggingOut(User user)
        {
            user = null;
            LoggingOut?.Invoke(this, new EventArgs());
        }

        #region Connection
        private void OnConnected(string hubConnectionString)
        {
            store.Add(CommonKeys.HubConnectionString.ToString(), hubConnectionString);
        }

        private void OnContactLoggedIn(User newOnlineUser)
        {
            List<User> contacts;
            if (!store.HasKey(CommonKeys.Contacts.ToString()))
                contacts = new List<User>();
            else 
                contacts = store.Get(CommonKeys.Contacts.ToString()) as List<User>;

            if (!contacts.Where(u => u.Id == newOnlineUser.Id).Any())
            {
                contacts.Add(newOnlineUser);
            }

            store.Add(CommonKeys.Contacts.ToString(), contacts);
            ContactLogged?.Invoke(this, new ContactLoggedEventArgs { User = newOnlineUser, IsLoggedIn = true });
        }

        private void OnContactLoggedOut(User disconnectedUser)
        {

           
            ContactLogged?.Invoke(this, new ContactLoggedEventArgs { User = disconnectedUser, IsLoggedIn = false });
            if (store.HasKey(CommonKeys.Chats.ToString()))
            {
                var chats = store.Get(CommonKeys.Chats.ToString()) as List<Chat>;
                var chat = chats.FirstOrDefault(c => c.Users.FirstOrDefault(u => u.Id == disconnectedUser.Id) != null);
                if (chat != null)
                {
                    chats.Remove(chat);
                }
            }
        }
        #endregion

        #region Chat
        private void OnChatCreated(Chat chat)
        {
            if (chat.Messages == null) chat.Messages = new List<Message>();
            if (store.HasKey(CommonKeys.Chats.ToString()))
            {
                var chats = store.Get(CommonKeys.Chats.ToString()) as List<Chat>;
                chats.Add(chat);
            }
            else
            {
                store.Add(CommonKeys.Chats.ToString(), new List<Chat> { chat });
            }
            var me = store.Get(CommonKeys.LoggedUser.ToString()) as User;
            var other = chat.Users.First(u => u.Id != me.Id);
            if (store.HasKey(CommonKeys.WithUser.ToString()) && (store.Get(CommonKeys.WithUser.ToString()) as User).Id == other.Id)
            {
                store.Add(CommonKeys.CurrentChat.ToString(), chat);
                var a = store.Get(CommonKeys.WithUser.ToString()) as User;
                ChatForUserRecived?.Invoke(chat, new ChatRecivedEventArgs { NewChat = chat, ContactName = a != null ? a.UserName : " " });
            }
        }
        #endregion

        #region Message
        private void OnMassageRecived(Message msg)
        {
            var chats = store.Get(CommonKeys.Chats.ToString()) as List<Chat>;
            var chat = chats.FirstOrDefault(c => c.Id == msg.ChatId);
            if (chat == null) return;//throw new Exception("Unhanadled Exception Chat not exist");
            if (chat.Messages == null) chat.Messages = new List<Message>();
            chat.Messages.Add(msg);
            MessageRecived?.Invoke(this, new MessageRecivedEventArgs { ChatId = chat.Id, Massage = msg });
        }

        #endregion

        #region Invites
        private void OnGameInvite(Chat chat)
        {
            var me = store.Get(CommonKeys.LoggedUser.ToString()) as User;
            var contact = chat.Users.First(u => u.Id != me.Id);
            UserInvitedToGame?.Invoke(this, new UserInvitedEventArgs { User = contact, ChatId = chat.Id });
        }

        private void OnGameAccepted(int chatId, bool isStarting)
        {
            //set chat as currnt chat.
            var localChat = (store.Get(CommonKeys.Chats.ToString()) as List<Chat>).First(c => c.Id == chatId);
            store.Add(CommonKeys.CurrentChat.ToString(), localChat);
            var me = store.Get(CommonKeys.LoggedUser.ToString()) as User;
            Debug.WriteLine($"{isStarting} { me.UserName}");
            store.Add(CommonKeys.WithUser.ToString(), localChat.Users.First(u => u.Id != me.Id));
            store.Add(CommonKeys.IsMyTurn.ToString(), isStarting);
            GameStarting?.Invoke(this, new GameStartingEventArgs { IsStarting = isStarting });
        }

        private void OnGameDenied(string msg)
        {
            Modal.ShowModal(msg);
        }

        #endregion

        #region Game

        private void OnPlayerPlayed(ActionUpdateModel obj)
        {
            OpponentPlayed?.Invoke(this, new OpponentPlayedEventArgs
            {
                Source = new MatrixLocation { Row = obj.SourceRow, Col = obj.SourceCol },
                Destenation = new MatrixLocation { Row = obj.DestenationRow, Col = obj.DestenationCol }
            });
        }
        private void OnPlayerFinnishedPlay()
        {
            OpponentFinnishedPlay?.Invoke(this, new EventArgs());
        }

        private void OnPlayerForfeit()
        {
            GameEnded?.Invoke(this, new EventArgs());
        }

        private void OnGameOver()
        {
            Modal.ShowModal("Better Luck Next Time", "GameOver!");
            GameEnded?.Invoke(this, new EventArgs());
        }

        #endregion
    }
}
