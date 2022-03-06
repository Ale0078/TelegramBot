using Bot.Data;

namespace Bot.Models
{
    public class ChatUser
    {
        public Guid Id { get; set; }

        public string UserName { get; set; }

        public string FirstName { get; set; }

        public string Surname { get; set; }

        public UserComingResource From { get; set; }

        public Chat Chat { get; set; }
    }
}
