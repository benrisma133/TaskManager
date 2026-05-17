// Repository/Repositories/TimerRepository.cs
using Microsoft.Data.SqlClient;
using Repository.Data;
using Repository.Loggers;
using Repository.Mappers;
using Repository.Models;
using System.Data;

namespace Repository.Repositories
{
    public static class TimerRepository
    {
        private static string ConnectionString
            => DatabaseHelper.ConnectionString;

        // ======================== [ START SESSION ] ========================
        public static int StartSession(int taskId)
        {
            try
            {
                using var conn = new SqlConnection(ConnectionString);
                using var cmd = new SqlCommand("sp_StartTimerSession", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@TaskId", taskId);

                conn.Open();

                using var reader = cmd.ExecuteReader();

                if (reader.Read())
                    return Convert.ToInt32(reader["NewSessionId"]);

                return 0;
            }
            catch (SqlException ex)
            {
                clsLog.LogError(nameof(TimerRepository), nameof(StartSession), ex);
                throw;
            }
            catch (Exception ex)
            {
                clsLog.LogError(nameof(TimerRepository), nameof(StartSession), ex);
                throw;
            }
        }

        // ======================== [ END SESSION ] ========================
        public static bool EndSession(int sessionId, int totalPausedSeconds)
        {
            try
            {
                using var conn = new SqlConnection(ConnectionString);
                using var cmd = new SqlCommand("sp_EndTimerSession", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@SessionId", sessionId);
                cmd.Parameters.AddWithValue("@TotalPausedSeconds", totalPausedSeconds);

                conn.Open();
                cmd.ExecuteNonQuery();

                return true;
            }
            catch (SqlException ex)
            {
                clsLog.LogError(nameof(TimerRepository), nameof(EndSession), ex);
                throw;
            }
            catch (Exception ex)
            {
                clsLog.LogError(nameof(TimerRepository), nameof(EndSession), ex);
                throw;
            }
        }

        // ======================== [ GET TODAY SESSIONS ] ========================
        public static (List<TimerSession> sessions, int totalSecondsToday, int totalMinutesToday) GetTodaySessionsByTask(int taskId)
        {
            var list = new List<TimerSession>();
            int totalSecondsToday = 0;
            int totalMinutesToday = 0;

            try
            {
                using var conn = new SqlConnection(ConnectionString);
                using var cmd = new SqlCommand("sp_GetTodaySessionsByTask", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@TaskId", taskId);

                conn.Open();

                using var reader = cmd.ExecuteReader();

                // ── First result set : sessions ───────────────────────
                while (reader.Read())
                {
                    list.Add(TimerSessionMapper.MapTimerSession(reader));
                }

                // ── Second result set : totals ────────────────────────
                if (reader.NextResult() && reader.Read())
                {
                    totalSecondsToday = reader.GetInt32(reader.GetOrdinal("TotalSecondsToday"));
                    totalMinutesToday = reader.GetInt32(reader.GetOrdinal("TotalMinutesToday"));
                }
            }
            catch (SqlException ex)
            {
                clsLog.LogError(nameof(TimerRepository), nameof(GetTodaySessionsByTask), ex);
                throw;
            }
            catch (Exception ex)
            {
                clsLog.LogError(nameof(TimerRepository), nameof(GetTodaySessionsByTask), ex);
                throw;
            }

            return (list, totalSecondsToday, totalMinutesToday);
        }

        // ======================== [ GET TASK TIMER SUMMARY ] ========================
        public static (int totalSecondsAllTime, int totalSecondsToday,
       List<TimerSession> pastSessions) GetTaskTimerSummary(int taskId)
        {
            int totalSecondsAllTime = 0;
            int totalSecondsToday = 0;
            var pastSessions = new List<TimerSession>();

            try
            {
                using var conn = new SqlConnection(ConnectionString);
                using var cmd = new SqlCommand("sp_GetTaskTimerSummary", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@TaskId", taskId);
                conn.Open();

                using var reader = cmd.ExecuteReader();

                // ── FIRST result set: past sessions ──────────────
                while (reader.Read())
                {
                    pastSessions.Add(TimerSessionMapper.MapTimerSession(reader));
                }

                // ── SECOND result set: totals ───────────────────
                if (reader.NextResult() && reader.Read())
                {
                    totalSecondsAllTime = reader.GetInt32(reader.GetOrdinal("TotalSecondsAllTime"));
                    totalSecondsToday = reader.GetInt32(reader.GetOrdinal("TotalSecondsToday"));
                }
            }
            catch (SqlException ex)
            {
                clsLog.LogError(nameof(TimerRepository), nameof(GetTaskTimerSummary), ex);
                throw;
            }

            return (totalSecondsAllTime, totalSecondsToday, pastSessions);
        }
    }
}