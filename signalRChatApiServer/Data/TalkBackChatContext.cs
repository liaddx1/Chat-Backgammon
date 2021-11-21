using Microsoft.EntityFrameworkCore;
using signalRChatApiServer.Models;
using System;

namespace signalRChatApiServer.Data
{
    public class TalkBackChatContext : DbContext
    {
        public TalkBackChatContext(DbContextOptions<TalkBackChatContext> options) : base(options) { }

        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Chat> Chats { get; set; }
        public virtual DbSet<Message> Messages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Chat>().HasData(
                new { Id = 1, InviteStatus = InviteStatus.Empty },
                new { Id = 2, InviteStatus = InviteStatus.Empty },
                new { Id = 3, InviteStatus = InviteStatus.Empty }
                );

            modelBuilder.Entity<Chat>(b =>
            {
                b.HasKey(e => e.Id);
                b.Property(e => e.Id).UseIdentityColumn();
            });

            modelBuilder.Entity<User>().HasData(
                    new { Id = 1, Status = Status.Offline, UserName = "User1", Password = "123", HubConnectionString = "dummy-c-string" },
                    new { Id = 2, Status = Status.Offline, UserName = "User2", Password = "123", HubConnectionString = "dummy-c-string" },
                    new { Id = 3, Status = Status.Offline, UserName = "User3", Password = "123", HubConnectionString = "dummy-c-string" }
                    );
            modelBuilder.Entity<Message>().HasData(
                new { Id = 1, Content = "למה לא בעצם?", Date = DateTime.Now.AddDays(2), ChatId = 1, Name = "User1", },
                new { Id = 2, Content = "למה אתה כותב באנגלית?", Date = DateTime.Now.AddMinutes(3), ChatId = 1, Name = "User2", },
                new { Id = 3, Content = "Hi, how are you?", Date = DateTime.Now.AddMinutes(4), ChatId = 1, Name = "User1", },

                new { Id = 4, Content = "Q_Q everyone are against me", Date = DateTime.Now.AddDays(2), ChatId = 2, Name = "User1", },
                new { Id = 5, Content = "why are you so toxic?", Date = DateTime.Now.AddDays(3), ChatId = 2, Name = "User3", },
                new { Id = 6, Content = "User2 is such a bitch...", Date = DateTime.Now.AddDays(4), ChatId = 2, Name = "User1", },

                new { Id = 7, Content = "מה... איזה דביל", Date = DateTime.Now.AddMinutes(5), ChatId = 3, Name = "User2", },
                new { Id = 8, Content = "כי שאלתי למה הוא כותב באנגלית", Date = DateTime.Now.AddMinutes(6), ChatId = 3, Name = "User3", },
                new { Id = 9, Content = "למה יוזר1 אחד שונא אותך?", Date = DateTime.Now, ChatId = 3, Name = "User2", }
                );
            modelBuilder.Entity<Chat>()
                        .HasMany(c => c.Users)
                        .WithMany(u => u.Chats)
                        .UsingEntity<ChatUser>(
                            cu => cu.HasOne(cu => cu.User)
                                    .WithMany(u => u.ChatUsers)
                                    .HasForeignKey(cu => cu.UserId),
                            cu => cu.HasOne(cu => cu.Chat)
                                    .WithMany(c => c.ChatUsers)
                                    .HasForeignKey(cu => cu.ChatId),
                            cu =>
                            {
                                cu.HasKey(cu => new { cu.ChatId, cu.UserId });
                                cu.HasData(
                                    new { ChatId = 1, UserId = 1 },
                                    new { ChatId = 1, UserId = 2 },

                                    new { ChatId = 2, UserId = 1 },
                                    new { ChatId = 2, UserId = 3 },

                                    new { ChatId = 3, UserId = 2 },
                                    new { ChatId = 3, UserId = 3 }
                                );
                            }
                        );
        }
    }
}
