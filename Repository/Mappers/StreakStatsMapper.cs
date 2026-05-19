using Microsoft.Data.SqlClient;
using Repository.Models;

namespace Repository.Mappers
{
    public static class StreakStatsMapper
    {
        public static StreakStats Map(SqlDataReader reader) =>
            new StreakStats
            {
                LongestStreak = reader.GetInt32(reader.GetOrdinal("LongestStreak")),
                CurrentStreak = reader.GetInt32(reader.GetOrdinal("CurrentStreak"))
            };
    }
}