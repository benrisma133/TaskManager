using Microsoft.Data.SqlClient;
using Repository.Models;

namespace Repository.Mappers
{
    public static class TodaySessionMapper
    {
        public static TodaySession Map(SqlDataReader reader) =>
            new TodaySession
            {
                SessionId = reader.GetInt32(reader.GetOrdinal("SessionId")),
                TaskId = reader.GetInt32(reader.GetOrdinal("TaskId")),
                TaskTitle = reader.GetString(reader.GetOrdinal("TaskTitle")),
                CategoryColor = reader.GetString(reader.GetOrdinal("CategoryColor")),
                StartTime = reader.GetDateTime(reader.GetOrdinal("StartTime")),
                EndTime = reader.GetDateTime(reader.GetOrdinal("EndTime")),
                DurationSeconds = reader.GetInt32(reader.GetOrdinal("DurationSeconds")),
                DurationMinutes = reader.GetInt32(reader.GetOrdinal("DurationMinutes"))
            };
    }
}