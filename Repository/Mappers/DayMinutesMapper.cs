using Microsoft.Data.SqlClient;
using Repository.Models;

namespace Repository.Mappers
{
    public static class DayMinutesMapper
    {
        public static DayMinutes Map(SqlDataReader reader) =>
            new DayMinutes
            {
                LogDate = reader.GetDateTime(reader.GetOrdinal("LogDate")),
                DayLabel = reader.GetString(reader.GetOrdinal("DayLabel")),
                IsToday = reader.GetInt32(reader.GetOrdinal("IsToday")) == 1,
                TotalMinutes = reader.GetInt32(reader.GetOrdinal("TotalMinutes"))
            };
    }
}