using Service.Utils;

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
        public DateOnly? StartDate { get; set; }
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

        // استعانة بـ DateHelper لحساب الأيام
        public int DaysLeft => DateHelper.GetDaysLeft(DueDate);

        // استعانة بـ DateHelper لجلب النص النسبي (أيام، أسابيع، أشهر، سنوات)
        public string DaysLeftText => DateHelper.GetRelativeTimeText(DueDate);

        public int TotalEstimatedMinutes =>
            (EstimatedMinutes ?? 0) + ExtraMinutes;
    }
}
