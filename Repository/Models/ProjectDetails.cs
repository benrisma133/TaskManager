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
        public string CategoryName { get; set; } = null!;
    }
}
