using System;
using System.Windows;
using tWpfMashUp_v0._0._1.Sevices;
using Microsoft.Extensions.Hosting;
using tWpfMashUp_v0._0._1.MVVM.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using tWpfMashUp_v0._0._1.MVVM.Models.GameModels;
using tWpfMashUp_v0._0._1.MVVM.Models.GameModels.Interfaces;

namespace tWpfMashUp_v0._0._1
{
    public partial class App : Application
    {
        public static IServiceProvider ServiceProvider { get; set; }
        private IHost host;

        private async void Application_Startup(object sender, StartupEventArgs e)
        {
            host = Host.CreateDefaultBuilder()
                    .ConfigureServices(ConfigServices)
                    .Build();
            await host.StartAsync();
            Start(host.Services);
        }

        private void ConfigServices(HostBuilderContext context, IServiceCollection services)
        {
            services.AddSingleton<MainWindow>();

            services.AddScoped<MainViewModel>();
            services.AddScoped<ChatAppViewModel>();
            services.AddScoped<ChatThreadViewModel>();

            services.AddTransient<LoginViewModel>();
            services.AddTransient<GameViewModel>();

            services.AddSingleton<StoreService>();
            services.AddSingleton<GameService>();

            services.AddScoped<AuthenticationService>();
            services.AddScoped<SignalRListenerService>();

            services.AddTransient<MessagesService>();
            services.AddTransient<ChatsService>();
            services.AddTransient<InvitesService>();
            services.AddTransient<IGameBoard, GameBoard>();
        }

        private static void Start(IServiceProvider services)
        {
            ServiceProvider = services;
            ServiceProvider.GetService<MainWindow>().Show();
        }

        protected async override void OnExit(ExitEventArgs e)
        {
            using (host) await host.StopAsync(TimeSpan.FromSeconds(2));
            base.OnExit(e);
        }
    }
}
//Ergent
//TODO: Export on server: hub logic to services; ☻ -> later
//TODO: if user has pending request he cannot Recive another one while waiting 
//TODO: if user is in a game he can not recive more game invites
//TODO: add turn taking logic for the game
//TODO: LogOut functionality.