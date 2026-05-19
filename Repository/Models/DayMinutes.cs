namespace Repository.Models
{
    public class DayMinutes
    {
        public DateTime LogDate { get; set; }
        public string DayLabel { get; set; } = string.Empty;
        public bool IsToday { get; set; }
        public int TotalMinutes { get; set; }
    }
}