namespace Bot.Models
{
    public class Chat
    {
        public long Id { get; set; }

        public bool DoesGetMail { get; set; }

        public bool DoesFinishTest { get; set; }

        public ChatUser ChatUser { get; set; }
    }
}
