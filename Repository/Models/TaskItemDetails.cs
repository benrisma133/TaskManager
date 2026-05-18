namespace Repository.Models
{
    public class TaskItemDetails
    {
        public int TaskID { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public int ProjectID { get; set; }
        public string Priority { get; set; } = null!;
        public string Status { get; set; } = null!;
        public DateOnly? DueDate { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? CompletedAt { get; set; }
        public int? EstimatedMinutes { get; set; }
        public int ExtraMinutes { get; set; } = 0;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string ProjectTitle { get; set; } = null!;
        public string? Color { get; set; }   // ← ADD THIS

        // ─── Computed Properties ───────────────────────────────────────────────
        public string EstimatedText => EstimatedMinutes.HasValue
            ? EstimatedMinutes.Value switch
            {
                < 60 => $"{EstimatedMinutes}m",
                _ => $"{EstimatedMinutes.Value / 60}h {EstimatedMinutes.Value % 60}m"
            }
            : "No estimate";

        public bool IsOverdue => DueDate.HasValue
            && DateOnly.FromDateTime(DateTime.Today) > DueDate.Value
            && !IsCompleted;

        public int DaysLeft => DueDate.HasValue
            ? DueDate.Value.DayNumber - DateOnly.FromDateTime(DateTime.Today).DayNumber
            : 0;

        public string DaysLeftText => DueDate.HasValue
            ? DaysLeft switch
            {
                < 0 => $"{Math.Abs(DaysLeft)}d overdue",
                0 => "Due today",
                1 => "Due tomorrow",
                _ => $"{DaysLeft}d left"
            }
            : "No due date";

        public int TotalEstimatedMinutes =>
            (EstimatedMinutes ?? 0) + ExtraMinutes;
    }
}
