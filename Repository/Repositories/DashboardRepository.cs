using Microsoft.Data.SqlClient;
using Repository.Data;
using Repository.Loggers;
using Repository.Mappers;
using Repository.Models;
using System.Data;

namespace Repository.Repositories
{
    public static class DashboardRepository
    {
        private static string ConnectionString =>
            DatabaseHelper.ConnectionString;

        // ======================== [ GET DASHBOARD STATS ] ========================
        public static DashboardStats GetDashboardStats()
        {
            var stats = new DashboardStats();

            try
            {
                using var conn = new SqlConnection(ConnectionString);
                using var cmd = new SqlCommand("sp_GetDashboardStats", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                conn.Open();

                using var reader = cmd.ExecuteReader();

                // ── Result Set 1: Completion counts ───────────────────
                if (reader.Read())
                    DashboardStatsMapper.MapCompletions(reader, stats);

                // ── Result Set 2: Points + Minutes ────────────────────
                reader.NextResult();
                if (reader.Read())
                    DashboardStatsMapper.MapPoints(reader, stats);
            }
            catch (SqlException ex)
            {
                clsLog.LogError(nameof(DashboardRepository), nameof(GetDashboardStats), ex);
                throw;
            }
            catch (Exception ex)
            {
                clsLog.LogError(nameof(DashboardRepository), nameof(GetDashboardStats), ex);
                throw;
            }

            return stats;
        }

        // ======================== [ GET STREAKS ] ========================
        public static StreakStats GetStreaks()
        {
            try
            {
                using var conn = new SqlConnection(ConnectionString);
                using var cmd = new SqlCommand("sp_GetStreaks", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                conn.Open();

                using var reader = cmd.ExecuteReader();
                return reader.Read() ? StreakStatsMapper.Map(reader) : new StreakStats();
            }
            catch (SqlException ex)
            {
                clsLog.LogError(nameof(DashboardRepository), nameof(GetStreaks), ex);
                throw;
            }
            catch (Exception ex)
            {
                clsLog.LogError(nameof(DashboardRepository), nameof(GetStreaks), ex);
                throw;
            }
        }

        // ======================== [ GET USER LEVEL ] ========================
        public static UserLevel? GetUserLevel()
        {
            try
            {
                using var conn = new SqlConnection(ConnectionString);
                using var cmd = new SqlCommand("sp_GetUserLevel", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                conn.Open();

                using var reader = cmd.ExecuteReader();
                return reader.Read() ? UserLevelMapper.Map(reader) : null;
            }
            catch (SqlException ex)
            {
                clsLog.LogError(nameof(DashboardRepository), nameof(GetUserLevel), ex);
                throw;
            }
            catch (Exception ex)
            {
                clsLog.LogError(nameof(DashboardRepository), nameof(GetUserLevel), ex);
                throw;
            }
        }

        // ======================== [ GET WEEKLY MINUTES ] ========================
        public static List<DayMinutes> GetWeeklyMinutes()
        {
            var list = new List<DayMinutes>();

            try
            {
                using var conn = new SqlConnection(ConnectionString);
                using var cmd = new SqlCommand("sp_GetWeeklyMinutes", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                conn.Open();

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                    list.Add(DayMinutesMapper.Map(reader));
            }
            catch (SqlException ex)
            {
                clsLog.LogError(nameof(DashboardRepository), nameof(GetWeeklyMinutes), ex);
                throw;
            }
            catch (Exception ex)
            {
                clsLog.LogError(nameof(DashboardRepository), nameof(GetWeeklyMinutes), ex);
                throw;
            }

            return list;
        }

        // ======================== [ GET TODAY SESSIONS ] ========================
        public static List<TodaySession> GetTodaySessions()
        {
            var list = new List<TodaySession>();

            try
            {
                using var conn = new SqlConnection(ConnectionString);
                using var cmd = new SqlCommand("sp_GetTodaySessions", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                conn.Open();

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                    list.Add(TodaySessionMapper.Map(reader));
            }
            catch (SqlException ex)
            {
                clsLog.LogError(nameof(DashboardRepository), nameof(GetTodaySessions), ex);
                throw;
            }
            catch (Exception ex)
            {
                clsLog.LogError(nameof(DashboardRepository), nameof(GetTodaySessions), ex);
                throw;
            }

            return list;
        }
    }
}