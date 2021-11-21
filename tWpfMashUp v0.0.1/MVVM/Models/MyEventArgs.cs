using System;
using tWpfMashUp_v0._0._1.MVVM.Models.GameModels;

namespace tWpfMashUp_v0._0._1.MVVM.Models
{

    public delegate void MessageRecivedEventHandler(object sender, MessageRecivedEventArgs eventArgs);
    public delegate void UserInvitedEventHandler(object sender, UserInvitedEventArgs eventArgs);
    public delegate void OpponentPlayedEventHandler(object sender, OpponentPlayedEventArgs e);
    public delegate void GameStartingEventHandler(object sender, GameStartingEventArgs e);
    public delegate void ContactLoggedEventHandler(object sender, ContactLoggedEventArgs e);

    public class MessageRecivedEventArgs : EventArgs
    {
        public Message Massage { get; set; }
        public int ChatId { get; set; }
    }

    public class ContactLoggedEventArgs : EventArgs
    {
        public User User { get; set; }
        public bool IsLoggedIn { get; set; }
    }

    public class UserInvitedEventArgs : EventArgs
    {
        public User User { get; set; }
        public int ChatId { get; set; }
    }

    public class ChatRecivedEventArgs : EventArgs
    {
        public Chat NewChat { get; set; }
        public string ContactName { get; set; }
    }

    public class OpponentPlayedEventArgs : EventArgs
    {
        public MatrixLocation Source { get; set; }
        public MatrixLocation Destenation { get; set; }
    }

    public class GameStartingEventArgs : EventArgs
    {
        public bool IsStarting { get; set; }
    }
}
