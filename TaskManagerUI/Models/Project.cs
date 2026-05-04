namespace TaskManagerUI.Models
{
    public class Project
    {
        public int ProjectID { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public int CategoryID { get; set; }
        public Category Category { get; set; } = null!;
        public string Status { get; set; } = null!;
        public string Priority { get; set; } = null!;
        public DateOnly? StartDate { get; set; }
        public DateOnly? DueDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
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
