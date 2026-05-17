// Repository/Models/TimerSession.cs
namespace Repository.Models
{
    public class TimerSession
    {
        public int SessionId { get; set; }
        public int TaskId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int? DurationSeconds { get; set; }
        public int TotalPausedSeconds { get; set; }
        public DateOnly? SessionDate { get; set; }
        public string? Notes { get; set; }
    }
}