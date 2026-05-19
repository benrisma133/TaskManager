namespace Repository.Models
{
    public class TodaySession
    {
        public int SessionId { get; set; }
        public int TaskId { get; set; }
        public string TaskTitle { get; set; } = string.Empty;
        public string CategoryColor { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int DurationSeconds { get; set; }
        public int DurationMinutes { get; set; }
    }
}