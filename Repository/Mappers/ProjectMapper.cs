using Microsoft.Data.SqlClient;
using Repository.Models;

namespace Repository.Mappers;

public static class ProjectMapper
{
    public static Project MapProject(SqlDataReader reader) =>
        new Project
        {
            ProjectID = reader.GetInt32(reader.GetOrdinal("ProjectId")),
            Title = reader.GetString(reader.GetOrdinal("Title")),
            Description = reader.IsDBNull(reader.GetOrdinal("Description"))
                            ? null
                            : reader.GetString(reader.GetOrdinal("Description")),
            CategoryID = reader.GetInt32(reader.GetOrdinal("CategoryId")),
            Status = reader.GetString(reader.GetOrdinal("Status")),
            Priority = reader.GetString(reader.GetOrdinal("Priority")),
            StartDate = reader.IsDBNull(reader.GetOrdinal("StartDate"))
                            ? null
                            : DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("StartDate"))),
            DueDate = reader.IsDBNull(reader.GetOrdinal("DueDate"))
                            ? null
                            : DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("DueDate"))),
            CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
            UpdatedAt = reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))
        };

    public static ProjectDetails MapProjectDetails(SqlDataReader reader)
    {
        return new ProjectDetails
        {
            ProjectID = reader.GetInt32(reader.GetOrdinal("ProjectId")),
            Title = reader.GetString(reader.GetOrdinal("Title")),
            Description = reader.IsDBNull(reader.GetOrdinal("Description"))
                                ? null
                                : reader.GetString(reader.GetOrdinal("Description")),
            CategoryID = reader.GetInt32(reader.GetOrdinal("CategoryId")),
            Status = reader.GetString(reader.GetOrdinal("Status")),
            Priority = reader.GetString(reader.GetOrdinal("Priority")),
            StartDate = reader.IsDBNull(reader.GetOrdinal("StartDate"))
                                ? null
                                : DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("StartDate"))),
            DueDate = reader.IsDBNull(reader.GetOrdinal("DueDate"))
                                ? null
                                : DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("DueDate"))),
            CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
            UpdatedAt = reader.GetDateTime(reader.GetOrdinal("UpdatedAt")),
            CategoryName = reader.GetString(reader.GetOrdinal("CategoryName")),
            CategoryColor = reader.GetString(reader.GetOrdinal("CategoryColor")),
            CategoryIcon = reader.GetString(reader.GetOrdinal("CategoryIcon")),
            TotalTasks = reader.GetInt32(reader.GetOrdinal("TotalTasks")),
            CompletedTasks = reader.GetInt32(reader.GetOrdinal("CompletedTasks"))
        };
    }

    public static ProjectLookup MapProjectLookup(SqlDataReader reader) =>
    new ProjectLookup
    {
        ProjectId = reader.GetInt32(reader.GetOrdinal("ProjectId")),
        Title = reader.GetString(reader.GetOrdinal("Title"))
    };

    public static ProjectPointsLog MapProjectPointsLog(SqlDataReader reader) =>
        new ProjectPointsLog
        {
            PointsLogId = reader.GetInt32(reader.GetOrdinal("PointsLogId")),
            Points = reader.GetInt32(reader.GetOrdinal("Points")),
            Reason = reader.GetString(reader.GetOrdinal("Reason")),
            TaskId = reader.IsDBNull(reader.GetOrdinal("TaskId"))
                              ? null
                              : reader.GetInt32(reader.GetOrdinal("TaskId")),
            ProjectId = reader.GetInt32(reader.GetOrdinal("ProjectId")),
            EarnedAt = reader.GetDateTime(reader.GetOrdinal("EarnedAt")),
            LogDate = DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("LogDate"))),
            TaskTitle = reader.IsDBNull(reader.GetOrdinal("TaskTitle"))
                              ? null
                              : reader.GetString(reader.GetOrdinal("TaskTitle"))
        };

    public static ProjectSessionSummary MapProjectSessionSummary(SqlDataReader reader) =>
        new ProjectSessionSummary
        {
            TaskId = reader.GetInt32(reader.GetOrdinal("TaskId")),
            TaskTitle = reader.GetString(reader.GetOrdinal("TaskTitle")),
            TotalSeconds = reader.GetInt32(reader.GetOrdinal("TotalSeconds")),
            LastSessionDate = DateOnly.FromDateTime(
                                  reader.GetDateTime(reader.GetOrdinal("LastSessionDate")))
        };

}