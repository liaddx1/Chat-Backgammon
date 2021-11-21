using signalRChatApiServer.Data;
using signalRChatApiServer.Hubs;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using signalRChatApiServer.Repositories.Infra;
using signalRChatApiServer.Repositories.Repos;
using Microsoft.Extensions.DependencyInjection;

namespace signalRChatApiServer
{
    public class Startup
    {
        private readonly IConfiguration configuration;

        public Startup(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IChatsRepository, ChatsReposatory>();
            services.AddTransient<IUsersRepository, UsersReposatory>();
            services.AddTransient<IMassegesReposatory, MassegesReposatory>();
            string connectionString = configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<TalkBackChatContext>(options => options.UseSqlServer(connectionString));
            services.AddSignalR();
            services.AddControllers();
        }

        public void Configure(IApplicationBuilder app, TalkBackChatContext ctx)
        {
            ctx.Database.EnsureDeleted();
            ctx.Database.EnsureCreated();

            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<ChatHub>("/ChatHub");
                endpoints.MapControllers();
            });
        }
    }
}
