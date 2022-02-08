using Bot.Models;

namespace Bot.Datas
{
    public class MarkScore
    {
        public int Score { get; set; }

        public int State { get; set; }

        public Question CurrentQuestion { get; set; }

        public bool IsCompleted { get; set; }
    }
}
