using Bot.Data;

namespace Bot.Entities
{
    public class ChatUser
    {
        public Guid Id { get; set; }

        public string UserName { get; set; }

        public string FirstName { get; set; }

        public string Surname { get; set; }

        public UserComingResource From { get; set; }

        public long ChatId { get; set; }

        public virtual Chat Chat { get; set; }
    }
}
