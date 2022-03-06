using Bot.Data;

namespace Bot.Models
{
    public class AdminUser
    {
        public Guid Id { get; set; }

        public string UserName { get; set; }

        public string FirstName { get; set; }

        public string Surname { get; set; }

        public UserRole Role { get; set; }
    }
}
