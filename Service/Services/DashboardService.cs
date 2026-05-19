using Repository.Models;
using Repository.Repositories;
using Service.Enums.Dashboard;

namespace Service.Services
{
    public class DashboardService
    {
        // ─── Fields ────────────────────────────────────────────────────────────
        private DashboardStats _stats = null!;
        private StreakStats _streaks = null!;
        private UserLevel? _level;
        private List<DayMinutes> _weeklyMinutes = new();
        private List<TodaySession> _todaySessions = new();

        // ─── Properties ────────────────────────────────────────────────────────

        // Stats
        public int CompletedToday => _stats.CompletedToday;
        public int CompletedLastWeek => _stats.CompletedLastWeek;
        public int CompletedLastMonth => _stats.CompletedLastMonth;
        public int CompletedAllTime => _stats.CompletedAllTime;
        public int TotalPoints => _stats.TotalPoints;
        public int PointsToday => _stats.PointsToday;
        public int MinutesToday => _stats.MinutesToday;

        // Streaks
        public int CurrentStreak => _streaks.CurrentStreak;
        public int LongestStreak => _streaks.LongestStreak;

        // Level
        public string CurrentLevel => _level?.CurrentLevel ?? "Beginner";
        public string CurrentBadge => _level?.CurrentBadge ?? "🌱";
        public string NextLevel => _level?.NextLevel ?? string.Empty;
        public string NextBadge => _level?.NextBadge ?? string.Empty;
        public int CurrentLevelPoints => _level?.CurrentLevelPoints ?? 0;
        public int NextLevelPoints => _level?.NextLevelPoints ?? 0;
        public int PointsToNextLevel => _level?.PointsToNextLevel ?? 0;
        public int ProgressPercent => _level?.ProgressPercent ?? 0;

        // Lists
        public List<DayMinutes> WeeklyMinutes => _weeklyMinutes;
        public List<TodaySession> TodaySessions => _todaySessions;

        // ─── Computed Properties ───────────────────────────────────────────────
        public string MinutesTodayFormatted
        {
            get
            {
                int h = MinutesToday / 60;
                int m = MinutesToday % 60;
                return h > 0 ? $"{h}h {m:00}m" : $"{m}m";
            }
        }

        public enGreeting Greeting
        {
            get
            {
                int hour = DateTime.Now.Hour;
                return hour switch
                {
                    < 12 => enGreeting.Morning,
                    < 17 => enGreeting.Afternoon,
                    _ => enGreeting.Evening
                };
            }
        }

        public string TodayLabel =>
            DateTime.Now.ToString("dddd, MMMM d, yyyy");

        public int MaxWeeklyMinutes =>
            _weeklyMinutes.Count > 0
                ? Math.Max(_weeklyMinutes.Max(d => d.TotalMinutes), 1)
                : 1;

        public bool IsMaxLevel => _level?.NextLevelPoints == 0;


        public enLevelType LevelType
        {
            get
            {
                return CurrentLevel switch
                {
                    "Beginner" => enLevelType.Beginner,
                    "Explorer" => enLevelType.Explorer,
                    "Builder" => enLevelType.Builder,
                    "Achiever" => enLevelType.Achiever,
                    "Expert" => enLevelType.Expert,
                    "Master" => enLevelType.Master,
                    "Legend" => enLevelType.Legend,
                    _ => enLevelType.Beginner
                };
            }
        }

        // ─── Constructor ───────────────────────────────────────────────────────
        public DashboardService()
        {
            _stats = new DashboardStats();
            _streaks = new StreakStats();
        }

        // ─── Static: Load ──────────────────────────────────────────────────────
        public static (enDashboardLoadResult result, DashboardService? service) Load()
        {
            try
            {
                var service = new DashboardService();

                service._stats = DashboardRepository.GetDashboardStats();
                service._streaks = DashboardRepository.GetStreaks();
                service._level = DashboardRepository.GetUserLevel();
                service._weeklyMinutes = DashboardRepository.GetWeeklyMinutes();
                service._todaySessions = DashboardRepository.GetTodaySessions();

                return (enDashboardLoadResult.Success, service);
            }
            catch
            {
                return (enDashboardLoadResult.Failed, null);
            }
        }

        // ─── Static: Refresh Stats Only ────────────────────────────────────────
        public static (enDashboardLoadResult result, DashboardStats? stats) RefreshStats()
        {
            try
            {
                var stats = DashboardRepository.GetDashboardStats();
                return (enDashboardLoadResult.Success, stats);
            }
            catch
            {
                return (enDashboardLoadResult.Failed, null);
            }
        }

        // ─── Static: Refresh Sessions Only ─────────────────────────────────────
        public static (enDashboardLoadResult result, List<TodaySession> sessions) RefreshSessions()
        {
            try
            {
                var sessions = DashboardRepository.GetTodaySessions();
                return (enDashboardLoadResult.Success, sessions);
            }
            catch
            {
                return (enDashboardLoadResult.Failed, new List<TodaySession>());
            }
        }
    }
}