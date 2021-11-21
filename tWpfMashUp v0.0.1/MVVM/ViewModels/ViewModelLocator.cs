using Microsoft.Extensions.DependencyInjection;

namespace tWpfMashUp_v0._0._1.MVVM.ViewModels
{
    public class ViewModelLocator
    {
        public MainViewModel Main => App.ServiceProvider.GetRequiredService<MainViewModel>();
        public LoginViewModel Authenticat => App.ServiceProvider.GetRequiredService<LoginViewModel>();
        public ChatAppViewModel Chat => App.ServiceProvider.GetRequiredService<ChatAppViewModel>();
        public ChatThreadViewModel ChatThread => App.ServiceProvider.GetRequiredService<ChatThreadViewModel>();
        public GameViewModel Game => App.ServiceProvider.GetRequiredService<GameViewModel>();

        public ViewModelLocator()
        {
            this.Chat.DisplayedUser = "";
        }
    }
}
