namespace Bot.Extentions
{
    public static class DateTimeExtentions
    {
        private const int OCTOBER_NAMBER = 10;
        private const int LAST_SUNDAY_OF_OCTODER = 30;
        private const int MARCH_NAMBER = 3;
        private const int LAST_SUNDAY_OF_MARCH = 27;

        public static DateTime GetUkrainianTimeFromUtc(this DateTime time) 
        {
            DateTime ukrainianDate;

            if (IsItWinterDate(time))
            {
                ukrainianDate = time.AddHours(2);
            }
            else 
            {
                ukrainianDate = time.AddHours(3);
            }

            return ukrainianDate;
        }

        private static bool IsItWinterDate(DateTime date) 
        {
            return date.Month > OCTOBER_NAMBER || date.Month < MARCH_NAMBER
                || (date.Month == OCTOBER_NAMBER && date.Day >= LAST_SUNDAY_OF_OCTODER)
                || (date.Month == MARCH_NAMBER && date.Day < LAST_SUNDAY_OF_MARCH);
        }
    }
}
