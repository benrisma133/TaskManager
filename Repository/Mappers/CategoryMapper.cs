using Microsoft.Data.SqlClient;
using Repository.Models;

namespace Repository.Mappers
{
    public static class CategoryMapper
    {
        public static Category Map(SqlDataReader reader) =>
            new Category
            {
                CategoryId = reader.GetInt32(reader.GetOrdinal("CategoryId")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                Color = reader.GetString(reader.GetOrdinal("Color")),
                Icon = reader.GetString(reader.GetOrdinal("Icon")),
                Type = reader.GetString(reader.GetOrdinal("Type")),
                IsCustom = reader.GetBoolean(reader.GetOrdinal("IsCustom"))
            };
    }
}
