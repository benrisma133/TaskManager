// Repository/Repositories/TimerRepository.cs
using Microsoft.Data.SqlClient;
using Repository.Data;
using Repository.Loggers;
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
        public static bool EndSession(int sessionId)
        {
            try
            {
                using var conn = new SqlConnection(ConnectionString);
                using var cmd = new SqlCommand("sp_EndTimerSession", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@SessionId", sessionId);

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
        public static (List<TimerSession> sessions, int totalMinutesToday) GetTodaySessionsByTask(int taskId)
        {
            var list = new List<TimerSession>();
            int totalMinutes = 0;

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
                    list.Add(new TimerSession
                    {
                        SessionId = reader.GetInt32(reader.GetOrdinal("SessionId")),
                        TaskId = reader.GetInt32(reader.GetOrdinal("TaskId")),
                        StartTime = reader.GetDateTime(reader.GetOrdinal("StartTime")),
                        EndTime = reader.IsDBNull(reader.GetOrdinal("EndTime"))
                                            ? null
                                            : reader.GetDateTime(reader.GetOrdinal("EndTime")),
                        DurationMinutes = reader.IsDBNull(reader.GetOrdinal("DurationMinutes"))
                                            ? null
                                            : reader.GetInt32(reader.GetOrdinal("DurationMinutes")),
                        SessionDate = DateOnly.FromDateTime(
                                            reader.GetDateTime(reader.GetOrdinal("SessionDate")))
                    });
                }

                // ── Second result set : total minutes ─────────────────
                if (reader.NextResult() && reader.Read())
                    totalMinutes = reader.GetInt32(reader.GetOrdinal("TotalMinutesToday"));
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

            return (list, totalMinutes);
        }

        public static (int totalSecondsAllTime, int totalSecondsToday,
                int totalMinutesAllTime, int totalMinutesToday,
                List<TimerSession> todaySessions) GetTaskTimerSummary(int taskId)
        {
            int totalSecsAllTime = 0;
            int totalSecsToday = 0;
            int totalMinsAllTime = 0;
            int totalMinsToday = 0;
            var sessions = new List<TimerSession>();

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

                if (reader.Read())
                {
                    totalSecsAllTime = reader.GetInt32(reader.GetOrdinal("TotalSecondsAllTime"));
                    totalMinsAllTime = reader.GetInt32(reader.GetOrdinal("TotalMinutesAllTime"));
                    totalSecsToday = reader.GetInt32(reader.GetOrdinal("TotalSecondsToday"));
                    totalMinsToday = reader.GetInt32(reader.GetOrdinal("TotalMinutesToday"));
                }

                if (reader.NextResult())
                {
                    while (reader.Read())
                    {
                        sessions.Add(new TimerSession
                        {
                            SessionId = reader.GetInt32(reader.GetOrdinal("SessionId")),
                            TaskId = reader.GetInt32(reader.GetOrdinal("TaskId")),
                            StartTime = reader.GetDateTime(reader.GetOrdinal("StartTime")),
                            EndTime = reader.IsDBNull(reader.GetOrdinal("EndTime"))
                                                ? null
                                                : reader.GetDateTime(reader.GetOrdinal("EndTime")),
                            DurationMinutes = reader.IsDBNull(reader.GetOrdinal("DurationMinutes"))
                                                ? null
                                                : reader.GetInt32(reader.GetOrdinal("DurationMinutes")),
                            DurationSeconds = reader.IsDBNull(reader.GetOrdinal("DurationSeconds"))
                                                ? null
                                                : reader.GetInt32(reader.GetOrdinal("DurationSeconds")),
                            SessionDate = DateOnly.FromDateTime(
                                                reader.GetDateTime(reader.GetOrdinal("SessionDate")))
                        });
                    }
                }
            }
            catch (SqlException ex)
            {
                clsLog.LogError(nameof(TimerRepository), nameof(GetTaskTimerSummary), ex);
                throw;
            }
            catch (Exception ex)
            {
                clsLog.LogError(nameof(TimerRepository), nameof(GetTaskTimerSummary), ex);
                throw;
            }

            return (totalSecsAllTime, totalSecsToday,
                    totalMinsAllTime, totalMinsToday, sessions);
        }
    }
}