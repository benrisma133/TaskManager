namespace Repository.Models
{
    public class UserLevel
    {
        public int LevelId { get; set; }
        public string CurrentLevel { get; set; } = string.Empty;
        public string CurrentBadge { get; set; } = string.Empty;
        public int CurrentLevelPoints { get; set; }
        public string NextLevel { get; set; } = string.Empty;
        public string NextBadge { get; set; } = string.Empty;
        public int NextLevelPoints { get; set; }
        public int TotalPoints { get; set; }
        public int PointsToNextLevel { get; set; }
        public int ProgressPercent { get; set; }
    }
}