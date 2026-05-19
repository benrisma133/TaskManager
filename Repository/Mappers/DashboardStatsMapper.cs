using Microsoft.Data.SqlClient;
using Repository.Models;

namespace Repository.Mappers
{
    public static class DashboardStatsMapper
    {
        // ── Maps Result Set 1 ─────────────────────────────────────
        public static void MapCompletions(SqlDataReader reader, DashboardStats stats)
        {
            stats.CompletedToday = reader.GetInt32(reader.GetOrdinal("CompletedToday"));
            stats.CompletedLastWeek = reader.GetInt32(reader.GetOrdinal("CompletedLastWeek"));
            stats.CompletedLastMonth = reader.GetInt32(reader.GetOrdinal("CompletedLastMonth"));
            stats.CompletedAllTime = reader.GetInt32(reader.GetOrdinal("CompletedAllTime"));
        }

        // ── Maps Result Set 2 ─────────────────────────────────────
        public static void MapPoints(SqlDataReader reader, DashboardStats stats)
        {
            stats.TotalPoints = reader.GetInt32(reader.GetOrdinal("TotalPoints"));
            stats.PointsToday = reader.GetInt32(reader.GetOrdinal("PointsToday"));
            stats.MinutesToday = reader.GetInt32(reader.GetOrdinal("MinutesToday"));
        }
    }
}