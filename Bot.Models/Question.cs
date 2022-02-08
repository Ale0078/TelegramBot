namespace Bot.Models
{
    public class Question
    {
        public Guid Id { get; set; }

        public string Content { get; set; }

        public int Mark { get; set; }

        public SuccessMessage SuccessMessage { get; set; }

        public FailMessage FailMessage { get; set; }

        public IList<Answer> Answers { get; set; }
    }
}