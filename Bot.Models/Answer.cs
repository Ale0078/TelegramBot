namespace Bot.Models
{
    public class Answer
    {
        public Guid Id { get; set; }

        public string Content { get; set; }

        public bool IsCorrect { get; set; }
    }
}
