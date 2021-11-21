using System;
using System.Windows;
using System.Windows.Input;
using tWpfMashUp_v0._0._1.Core;
using tWpfMashUp_v0._0._1.Sevices;
using tWpfMashUp_v0._0._1.MVVM.Views;

namespace tWpfMashUp_v0._0._1.MVVM.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        private readonly SignalRListenerService signalRListener;
        private readonly AuthenticationService authenticationService;
        public RelayCommand MinimizeCommand { get; set; }
        public RelayCommand MaximizeCommand { get; set; }
        public RelayCommand CloseCommand { get; set; }
        public RelayCommand MouseDownCommand { get; set; }

        private object view;
        public object View { get => view; set { view = value; onProppertyChange(); } }
        private object modal;
        public object Modal { get => modal; set { modal = value; onProppertyChange(); } }

        public MainViewModel(SignalRListenerService signalRListiner, AuthenticationService authenticationService)
        {
            MinimizeCommand = new RelayCommand(o => Application.Current.MainWindow.WindowState = WindowState.Minimized);
            MaximizeCommand = new RelayCommand(o => OnMaximizeCommand());
            CloseCommand = new RelayCommand(o => Application.Current.Shutdown());
            MouseDownCommand = new RelayCommand(o => OnMouseDown(o as MouseButtonEventArgs));
            this.authenticationService = authenticationService;
            signalRListener = signalRListiner;
            View = new LoginView();
            this.authenticationService.LoggingIn += (s, e) => SetViewTransition("Chat");
            this.signalRListener.GameStarting += (s, e) => SetViewTransition("Game");
            this.signalRListener.GameEnded += (s, e) => SetViewTransition("Chat");
            this.authenticationService.LoggingOut += (s, e) => SetViewTransition("Auth");
        }

        private void OnMouseDown(MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) Application.Current.MainWindow.DragMove();
        }

        private void OnMaximizeCommand()
        {
            Application.Current.MainWindow.WindowState = Application.Current.MainWindow.WindowState != WindowState.Maximized ? WindowState.Maximized : WindowState.Normal;
        }

        public void SetViewTransition(string option)
        {
            View = option switch
            {
                "Game" => new ChatAndGameView(),
                "Auth" => new LoginView(),
                "Chat" => new ChatAppView(),
                _ => new LoginView(),
            };
        }
    }
}
