namespace Repository.Models
{
    public class ProjectPointsLog
    {
        public int PointsLogId { get; set; }
        public int Points { get; set; }
        public string Reason { get; set; } = null!;
        public int? TaskId { get; set; }
        public int ProjectId { get; set; }
        public DateTime EarnedAt { get; set; }
        public DateOnly LogDate { get; set; }
        public string? TaskTitle { get; set; }
    }
}
