using System.Linq;
using System.Windows.Controls;
using tWpfMashUp_v0._0._1.Core;
using System.Collections.Generic;
using tWpfMashUp_v0._0._1.Sevices;
using System.Collections.ObjectModel;
using tWpfMashUp_v0._0._1.MVVM.Models;
using tWpfMashUp_v0._0._1.Assets.Components.CustomModal;
using System.Diagnostics;

namespace tWpfMashUp_v0._0._1.MVVM.ViewModels
{
    public class ChatAppViewModel : ObservableObject
    {
        //Fields
        private readonly SignalRListenerService signalRListinerService;
        private readonly AuthenticationService authenticationService;
        private readonly InvitesService gameService;
        private readonly StoreService storeService;
        private readonly ChatsService chatsService;

        //Full Props
        private User loggedUser;
        public User LoggedUser { get => loggedUser; set { loggedUser = value; onProppertyChange(); } }

        private Chat selectedChat;
        public Chat SelectedChat { get => selectedChat; set { selectedChat = value; onProppertyChange(); } }

        private string displayedUser;
        public string DisplayedUser { get => displayedUser; set { displayedUser = value; onProppertyChange(); } }

        private User selectedUser;
        public User SelectedUser { get => selectedUser; set { selectedUser = value; onProppertyChange(); } }

        private string bindingTest;
        public string BindingTest { get => bindingTest; set { bindingTest = value; onProppertyChange(); } }

        //Commands
        public RelayCommand OnSelectionChangedCommand { get; set; }
        public RelayCommand OnInviteToGameCommand { get; set; }
        public RelayCommand OnLogOutCommand { get; set; }
        public RelayCommand OnInformationCommand { get; set; }
        public RelayCommand OnAboutCommand { get; set; }

        //Ui Collections
        public ObservableCollection<User> OnlineContacts { get; set; }
        public ObservableCollection<User> OfflineContacts { get; set; }

        public ChatAppViewModel(StoreService store, ChatsService chatsService,
            AuthenticationService authenticationService, SignalRListenerService signalRListinerService, InvitesService gameService)
        {
            storeService = store;
            this.gameService = gameService;
            this.chatsService = chatsService;
            this.authenticationService = authenticationService;
            this.signalRListinerService = signalRListinerService;
            OfflineContacts = new ObservableCollection<User>();
            OnlineContacts = new ObservableCollection<User>();

            OnSelectionChangedCommand = new RelayCommand(o => HandleSelectionChanged(o as SelectionChangedEventArgs));
            OnInviteToGameCommand = new RelayCommand((o) => InviteToGame());
            OnInformationCommand = new RelayCommand((o) => ShowInformation());
            OnAboutCommand = new RelayCommand((o) => AboutTheDevs());
            OnLogOutCommand = new RelayCommand((o) => LogOut());

            this.authenticationService.LoggingIn += (s, e) => FetchUserHandler();
            this.signalRListinerService.ContactLogged += OnContactLogged;
            this.signalRListinerService.MessageRecived += OnMassageRecived;
        }

        private async void LogOut() => await authenticationService.InvokeLogOut();

        private async void ShowInformation()
        {
            var res = await Modal.ShowModal("You Are Sent To a Website Guide", "How to Play?", "OK", "Close");
            if (res == "OK")
                Process.Start(new ProcessStartInfo { FileName = "https://www.bkgm.com/rules.html", UseShellExecute = true });
        }

        private void AboutTheDevs()
        {
            Modal.ShowModal("This Software Is Brought To You By\nLiad Dadon and Shoham Siso.\nv 1.0.0", "About");
        }

        private async void InviteToGame() => await gameService.CallServerForOtherUserInvite();

        private void OnMassageRecived(object sender, MessageRecivedEventArgs eventArgs)
        {
            if (storeService.HasKey(CommonKeys.CurrentChat.ToString()))
            {
                var c = storeService.Get(CommonKeys.CurrentChat.ToString()) as Chat;
                if (eventArgs.ChatId == c.Id) { return; }
            }
            var contacts = (storeService.Get(CommonKeys.Contacts.ToString()) as List<User>);
            var contact = contacts.First(u => u.UserName == eventArgs.Massage.Name);
            contact.HasUnreadMessage = true;//false
            OnlineContacts.Remove(OnlineContacts.First(u => u.Id == contact.Id));
            OnlineContacts.Insert(0, contact);
        }

        private void FetchUserHandler()
        {
            LoggedUser = storeService.Get(CommonKeys.LoggedUser.ToString()) as User;
            DisplayedUser = $"{LoggedUser.UserName} (#{LoggedUser.Id})";
            FetchAllOnlineContacts();
        }

        private async void FetchAllOnlineContacts()
        {
            await authenticationService.FetchAllLoggedUsers();
            App.Current.Dispatcher.Invoke(() => UpdateUsersList());
        }

        private void UpdateUsersList()
        {
            var users = storeService.Get(CommonKeys.Contacts.ToString()) as List<User>;
            if (!users.Any()) return;
            OnlineContacts.Clear();
            OfflineContacts.Clear();
            foreach (var u in users)
            {
                if (u.Status == Status.Online) OnlineContacts.Add(u);
                else OfflineContacts.Add(u);
            }
        }

        private void OnContactLogged(object sender, System.EventArgs e)
        {
            var args = e as ContactLoggedEventArgs;
            if (args.IsLoggedIn) App.Current.Dispatcher.Invoke(() => OnContactLoggedIn(args.User));
            else App.Current.Dispatcher?.Invoke(() => OnContactLoggedOut(args.User));
        }

        private void OnContactLoggedIn(User user)
        {
            if (!OnlineContacts.Where(u => u.Id == user.Id).Any())
            {
                App.Current.Dispatcher.Invoke(() =>
                  {
                      OnlineContacts.Add(user);
                      OfflineContacts.Remove(OfflineContacts.FirstOrDefault(u => u.Id == user.Id));
                  });
            }
        }

        private void OnContactLoggedOut(User user)
        {
            if (!OfflineContacts.Where(u => u.Id == user.Id).Any())
            {
                App.Current.Dispatcher.Invoke(() =>
                    {
                        OfflineContacts.Add(user);
                        OnlineContacts.Remove(OnlineContacts.FirstOrDefault(u => u.Id == user.Id));
                    });
            }
        }

        public async void HandleSelectionChanged(SelectionChangedEventArgs selectionChangedEventArgs)
        {
            try
            {
                if (selectionChangedEventArgs.RemovedItems != null && selectionChangedEventArgs.RemovedItems.Count > 0)
                {
                    storeService.Remove(CommonKeys.CurrentChat.ToString());
                    storeService.Remove(CommonKeys.WithUser.ToString());
                }
                if (selectionChangedEventArgs.AddedItems != null && selectionChangedEventArgs.AddedItems.Count > 0)
                {
                    var newCurrentUser = selectionChangedEventArgs.AddedItems[0] as User;
                    storeService.Add(CommonKeys.WithUser.ToString(), newCurrentUser);
                    await chatsService.CreateChatIfNotExistAsync(newCurrentUser);
                    if (newCurrentUser.HasUnreadMessage)
                    {
                        newCurrentUser.HasUnreadMessage = false;
                        var user = OnlineContacts.First(u => u.Id == newCurrentUser.Id);
                        user.HasUnreadMessage = false;
                        OnlineContacts.Remove(user);
                        OnlineContacts.Add(newCurrentUser);
                        SelectedUser = user;
                    }
                }
            }
            catch { }
        }
    }
}
