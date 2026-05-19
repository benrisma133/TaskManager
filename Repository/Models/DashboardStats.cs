namespace Repository.Models
{
    public class DashboardStats
    {
        // ── Task Completion ───────────────────────────────────────
        public int CompletedToday { get; set; }
        public int CompletedLastWeek { get; set; }
        public int CompletedLastMonth { get; set; }
        public int CompletedAllTime { get; set; }

        // ── Points ────────────────────────────────────────────────
        public int TotalPoints { get; set; }
        public int PointsToday { get; set; }

        // ── Time ──────────────────────────────────────────────────
        public int MinutesToday { get; set; }
    }
}