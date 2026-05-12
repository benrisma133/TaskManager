namespace Repository.Models
{
    public class TaskItem
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
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
