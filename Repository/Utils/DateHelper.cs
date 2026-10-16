namespace Service.Utils
{
    public static class DateHelper
    {
        /// <summary>
        /// Calculates days remaining or overdue from a given due date.
        /// Positive = days left, Negative = days overdue, 0 = due today.
        /// </summary>
        public static int GetDaysLeft(DateOnly? dueDate)
        {
            if (!dueDate.HasValue) return 0;
            return dueDate.Value.DayNumber - DateOnly.FromDateTime(DateTime.Today).DayNumber;
        }

        /// <summary>
        /// Generates a human-readable relative time string (e.g., 5d overdue, 2w left, 3 months left, 1 year overdue).
        /// </summary>
        public static string GetRelativeTimeText(DateOnly? dueDate)
        {
            if (!dueDate.HasValue) return "No due date";

            int daysLeft = GetDaysLeft(dueDate);

            return daysLeft switch
            {
                0 => "Due today",
                1 => "Due tomorrow",
                -1 => "1d overdue",
                < -1 => FormatInterval(Math.Abs(daysLeft), isOverdue: true),
                _ => FormatInterval(daysLeft, isOverdue: false)
            };
        }

        private static string FormatInterval(int totalDays, bool isOverdue)
        {
            string suffix = isOverdue ? "overdue" : "left";

            if (totalDays < 7)
            {
                return $"{totalDays}d {suffix}";
            }
            else if (totalDays < 30)
            {
                int weeks = totalDays / 7;
                return weeks == 1 ? $"1w {suffix}" : $"{weeks}w {suffix}";
            }
            else if (totalDays < 365)
            {
                int months = totalDays / 30;
                return months == 1 ? $"1 month {suffix}" : $"{months} months {suffix}";
            }
            else
            {
                int years = totalDays / 365;
                return years == 1 ? $"1 year {suffix}" : $"{years} years {suffix}";
            }
        }
    }
}
