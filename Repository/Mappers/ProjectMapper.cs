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

}