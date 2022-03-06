using Bot.Data;

namespace Bot.Models
{
    public class AdminUser
    {
        public Guid Id { get; set; }

        public long AdminChatId { get; set; }

        public string UserName { get; set; }

        public UserRole Role { get; set; }
    }
}
