namespace Repository.Models
{
    public class ProjectDetails
    {
        public int ProjectID { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public int CategoryID { get; set; }
        public string Status { get; set; } = null!;
        public string Priority { get; set; } = null!;
        public DateOnly? StartDate { get; set; }
        public DateOnly? DueDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // ── From Category JOIN ─────────────────────────────────────────
        public string CategoryName { get; set; } = null!;
        public string CategoryColor { get; set; } = null!;
        public string CategoryIcon { get; set; } = null!;

        // ── Task Progress ──────────────────────────────────────────────
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }

        // ── Computed Properties ────────────────────────────────────────
        public int ProgressPercentage => TotalTasks > 0
            ? (int)Math.Round((double)CompletedTasks / TotalTasks * 100)
            : 0;

        public int DaysLeft => DueDate.HasValue
            ? DueDate.Value.DayNumber - DateOnly.FromDateTime(DateTime.Today).DayNumber
            : 0;

        public bool IsOverdue => DueDate.HasValue
            && DateOnly.FromDateTime(DateTime.Today) > DueDate.Value
            && Status != "Completed"
            && Status != "Archived";

        public string DaysLeftText => DueDate.HasValue
            ? DaysLeft switch
            {
                < 0 => $"{Math.Abs(DaysLeft)}d overdue",
                0 => "Due today",
                1 => "Due tomorrow",
                _ => $"{DaysLeft}d left"
            }
            : "No due date";
    }
}