﻿namespace Bot.Entities
{
    public class Mailing
    {
        public Guid Id { get; set; }

        public string Message { get; set; }

        public DateTime DateOfMailing { get; set; }
    }
}
