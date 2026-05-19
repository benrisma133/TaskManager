using Microsoft.Data.SqlClient;
using Repository.Models;

namespace Repository.Mappers
{
    public static class UserLevelMapper
    {
        public static UserLevel Map(SqlDataReader reader) =>
            new UserLevel
            {
                LevelId = reader.GetInt32(reader.GetOrdinal("LevelId")),
                CurrentLevel = reader.GetString(reader.GetOrdinal("CurrentLevel")),
                CurrentBadge = reader.GetString(reader.GetOrdinal("CurrentBadge")),
                CurrentLevelPoints = reader.GetInt32(reader.GetOrdinal("CurrentLevelPoints")),
                NextLevel = reader.IsDBNull(reader.GetOrdinal("NextLevel"))
                                         ? string.Empty
                                         : reader.GetString(reader.GetOrdinal("NextLevel")),
                NextBadge = reader.IsDBNull(reader.GetOrdinal("NextBadge"))
                                         ? string.Empty
                                         : reader.GetString(reader.GetOrdinal("NextBadge")),
                NextLevelPoints = reader.IsDBNull(reader.GetOrdinal("NextLevelPoints"))
                                         ? 0
                                         : reader.GetInt32(reader.GetOrdinal("NextLevelPoints")),
                TotalPoints = reader.GetInt32(reader.GetOrdinal("TotalPoints")),
                PointsToNextLevel = reader.GetInt32(reader.GetOrdinal("PointsToNextLevel")),
                ProgressPercent = reader.GetInt32(reader.GetOrdinal("ProgressPercent"))
            };
    }
}