//namespace TaskManagerUI.Models
//{
//    public class TimerSession
//    {
//        public int SessionId { get; set; }
//        public int TaskId { get; set; }
//        public DateTime StartTime { get; set; }
//        public DateTime? EndTime { get; set; }
//        public int? DurationMinutes { get; set; }
//        public DateOnly SessionDate { get; set; }
//        public string? Notes { get; set; }

//        public string StartTimeText => StartTime.ToString("hh:mm tt");
//        public string DurationText => DurationMinutes.HasValue
//            ? $"{DurationMinutes} min"
//            : "Running...";
//        public bool IsRunning => EndTime == null;
//    }
//}
