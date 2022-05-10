using Microsoft.EntityFrameworkCore;

namespace Bot.Entities
{
    public class ApplicationContext : DbContext
    {
        public DbSet<Question> Questions { get; set; }

        public DbSet<Answer> Answers { get; set; }

        public DbSet<SuccessMessage> SuccessMessages { get; set; }

        public DbSet<FailMessage> FailMessages { get; set; }

        public DbSet<Chat> Chats { get; set; }

        public DbSet<ChatUser> ChatUsers { get; set; }

        public DbSet<AdminUser> AdminUsers { get; set; }

        public DbSet<Mailing> Mailings { get; set; }

        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
        {
            //Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseLazyLoadingProxies();

            base.OnConfiguring(optionsBuilder);
        }
    }
}