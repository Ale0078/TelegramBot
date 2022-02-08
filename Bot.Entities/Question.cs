namespace Bot.Entities
{
    public class Question
    {
        public Guid Id { get; set; }

        public string Content { get; set; }

        public int Mark { get; set; }

        public Guid SuccessMessageId { get; set; }

        public Guid FailMessageId { get; set; }

        public virtual SuccessMessage SuccessMessage { get; set; }

        public virtual FailMessage FailMessage { get; set; }

        public virtual IList<Answer> Answers { get; set; }
    }
}
