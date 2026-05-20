namespace Repository.Models
{
    public class ProjectSessionSummary
    {
        public int TaskId { get; set; }
        public string TaskTitle { get; set; } = null!;
        public int TotalSeconds { get; set; }
        public DateOnly LastSessionDate { get; set; }

        public string FormattedTime
        {
            get
            {
                if (TotalSeconds < 60) return $"{TotalSeconds}s";
                int m = TotalSeconds / 60;
                if (m < 60) return $"{m}m";
                int h = m / 60;
                int rem = m % 60;
                return rem == 0 ? $"{h}h" : $"{h}h {rem}m";
            }
        }
    }
}
