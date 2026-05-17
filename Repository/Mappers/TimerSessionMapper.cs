using Microsoft.Data.SqlClient;
using Repository.Models;

namespace Repository.Mappers
{
    public static class TimerSessionMapper
    {
        public static TimerSession MapTimerSession(SqlDataReader reader)
        {
            return new TimerSession
            {
                SessionId = reader.GetInt32(reader.GetOrdinal("SessionId")),
                TaskId = reader.GetInt32(reader.GetOrdinal("TaskId")),
                StartTime = reader.GetDateTime(reader.GetOrdinal("StartTime")),
                EndTime = reader.IsDBNull(reader.GetOrdinal("EndTime"))
                    ? null
                    : reader.GetDateTime(reader.GetOrdinal("EndTime")),
                DurationSeconds = reader.IsDBNull(reader.GetOrdinal("DurationSeconds"))
                    ? null
                    : reader.GetInt32(reader.GetOrdinal("DurationSeconds")),
                TotalPausedSeconds = reader.IsDBNull(reader.GetOrdinal("TotalPausedSeconds"))
                    ? 0
                    : reader.GetInt32(reader.GetOrdinal("TotalPausedSeconds"))
            };
        }
    }
}
