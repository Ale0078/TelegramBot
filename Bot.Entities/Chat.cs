namespace Bot.Entities
{
    public class Chat
    {
        public long Id { get; set; }

        public bool DoesGetMail { get; set; }

        public bool DoesFinishTest { get; set; }

        public virtual ChatUser ChatUser { get; set; }
    }
}
