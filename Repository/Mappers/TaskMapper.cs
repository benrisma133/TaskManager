using Microsoft.Data.SqlClient;
using Repository.Models;

namespace Repository.Mappers;

public static class TaskMapper
{
    public static TaskItem MapTaskItem(SqlDataReader reader) =>
        new TaskItem
        {
            TaskID = reader.GetInt32(reader.GetOrdinal("TaskId")),
            Title = reader.GetString(reader.GetOrdinal("Title")),
            Description = reader.IsDBNull(reader.GetOrdinal("Description"))
                                    ? null
                                    : reader.GetString(reader.GetOrdinal("Description")),
            ProjectID = reader.GetInt32(reader.GetOrdinal("ProjectId")),
            Status = reader.GetString(reader.GetOrdinal("Status")),
            Priority = reader.GetString(reader.GetOrdinal("Priority")),
            DueDate = reader.IsDBNull(reader.GetOrdinal("DueDate"))
                                    ? null
                                    : DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("DueDate"))),
            IsCompleted = reader.GetBoolean(reader.GetOrdinal("IsCompleted")),
            CompletedAt = reader.IsDBNull(reader.GetOrdinal("CompletedAt"))
                                    ? null
                                    : reader.GetDateTime(reader.GetOrdinal("CompletedAt")),
            EstimatedMinutes = reader.IsDBNull(reader.GetOrdinal("EstimatedMinutes"))
                                    ? null
                                    : reader.GetInt32(reader.GetOrdinal("EstimatedMinutes")),
            ExtraMinutes = reader["ExtraMinutes"] != DBNull.Value
                                    ? Convert.ToInt32(reader["ExtraMinutes"])
                                    : 0,
            CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
            UpdatedAt = reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))
        };

    public static TaskItemDetails MapTaskItemDetails(SqlDataReader reader) =>
    new TaskItemDetails
    {
        TaskID = reader.GetInt32(reader.GetOrdinal("TaskId")),
        Title = reader.GetString(reader.GetOrdinal("Title")),
        Description = reader.IsDBNull(reader.GetOrdinal("Description"))
                            ? null
                            : reader.GetString(reader.GetOrdinal("Description")),
        ProjectID = reader.GetInt32(reader.GetOrdinal("ProjectId")),
        Status = reader.GetString(reader.GetOrdinal("Status")),
        Priority = reader.GetString(reader.GetOrdinal("Priority")),
        DueDate = reader.IsDBNull(reader.GetOrdinal("DueDate"))
                            ? null
                            : DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("DueDate"))),
        IsCompleted = reader.GetBoolean(reader.GetOrdinal("IsCompleted")),
        CompletedAt = reader.IsDBNull(reader.GetOrdinal("CompletedAt"))
                            ? null
                            : reader.GetDateTime(reader.GetOrdinal("CompletedAt")),
        EstimatedMinutes = reader.IsDBNull(reader.GetOrdinal("EstimatedMinutes"))
                            ? null
                            : reader.GetInt32(reader.GetOrdinal("EstimatedMinutes")),
        ExtraMinutes = reader.IsDBNull(reader.GetOrdinal("ExtraMinutes"))
                            ? 0
                            : reader.GetInt32(reader.GetOrdinal("ExtraMinutes")),
        CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
        UpdatedAt = reader.GetDateTime(reader.GetOrdinal("UpdatedAt")),
        ProjectTitle = reader.GetString(reader.GetOrdinal("ProjectTitle")),
        Color = reader.IsDBNull(reader.GetOrdinal("Color"))   // ← ADD THIS
                            ? null
                            : reader.GetString(reader.GetOrdinal("Color"))
    };
}