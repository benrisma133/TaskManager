using Microsoft.Data.SqlClient;
using Repository.Data;
using Repository.Loggers;
using Repository.Mappers;
using Repository.Models;
using System.Data;

namespace Repository.Repositories;

public static class ProjectRepository
{
    private static string ConnectionString =>
        DatabaseHelper.ConnectionString;

    // ======================== [ ADD PROJECT ] ========================
    public static int AddProject(Project project)
    {
        try
        {
            using var conn = new SqlConnection(ConnectionString);
            using var cmd = new SqlCommand("sp_AddProject", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@Title", project.Title);
            cmd.Parameters.AddWithValue("@Description", (object?)project.Description ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CategoryId", project.CategoryID);
            cmd.Parameters.AddWithValue("@Priority", project.Priority);
            cmd.Parameters.AddWithValue("@StartDate", (object?)project.StartDate?.ToDateTime(TimeOnly.MinValue) ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@DueDate", (object?)project.DueDate?.ToDateTime(TimeOnly.MinValue) ?? DBNull.Value);

            conn.Open();

            var result = cmd.ExecuteScalar();
            return result is not null ? Convert.ToInt32(result) : -1;
        }
        catch (SqlException ex)
        {
            clsLog.LogError(nameof(ProjectRepository), nameof(AddProject), ex);
            throw;
        }
        catch (Exception ex)
        {
            clsLog.LogError(nameof(ProjectRepository), nameof(AddProject), ex);
            throw;
        }
    }

    // ======================== [ UPDATE PROJECT ] ========================
    public static bool UpdateProject(Project project)
    {
        try
        {
            using var conn = new SqlConnection(ConnectionString);
            using var cmd = new SqlCommand("sp_UpdateProject", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@ProjectId", project.ProjectID);
            cmd.Parameters.AddWithValue("@Title", project.Title);
            cmd.Parameters.AddWithValue("@Description", (object?)project.Description ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CategoryId", project.CategoryID);
            cmd.Parameters.AddWithValue("@Priority", project.Priority);
            cmd.Parameters.AddWithValue("@Status", project.Status);
            cmd.Parameters.AddWithValue("@StartDate", (object?)project.StartDate?.ToDateTime(TimeOnly.MinValue) ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@DueDate", (object?)project.DueDate?.ToDateTime(TimeOnly.MinValue) ?? DBNull.Value);

            conn.Open();
            return cmd.ExecuteNonQuery() > 0;
        }
        catch (SqlException ex)
        {
            clsLog.LogError(nameof(ProjectRepository), nameof(UpdateProject), ex);
            throw;
        }
        catch (Exception ex)
        {
            clsLog.LogError(nameof(ProjectRepository), nameof(UpdateProject), ex);
            throw;
        }
    }

    // ======================== [ HARD DELETE PROJECT ] ========================
    public static void HardDeleteProject(int projectId)
    {
        try
        {
            using var conn = new SqlConnection(ConnectionString);
            using var cmd = new SqlCommand("sp_HardDeleteProject", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@ProjectId", projectId);
            conn.Open();
            cmd.ExecuteNonQuery();
        }
        catch (SqlException ex)
        {
            clsLog.LogError(nameof(ProjectRepository), nameof(HardDeleteProject), ex);
            throw;
        }
        catch (Exception ex)
        {
            clsLog.LogError(nameof(ProjectRepository), nameof(HardDeleteProject), ex);
            throw;
        }
    }

    // ======================== [ SOFT DELETE PROJECT ] ========================
    public static string SoftDeleteProject(int projectId)
    {
        try
        {
            using var conn = new SqlConnection(ConnectionString);
            using var cmd = new SqlCommand("sp_SoftDeleteProject", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@ProjectId", projectId);
            conn.Open();

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
                return reader.GetString(reader.GetOrdinal("Result"));

            return "UNKNOWN";
        }
        catch (SqlException ex)
        {
            clsLog.LogError(nameof(ProjectRepository), nameof(SoftDeleteProject), ex);
            throw;
        }
        catch (Exception ex)
        {
            clsLog.LogError(nameof(ProjectRepository), nameof(SoftDeleteProject), ex);
            throw;
        }
    }

    // ======================== [ GET PROJECT BY ID ] ========================
    public static Project? GetProjectById(int projectId)
    {
        try
        {
            using var conn = new SqlConnection(ConnectionString);
            using var cmd = new SqlCommand("sp_GetProjectById", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@ProjectId", projectId);
            conn.Open();

            using var reader = cmd.ExecuteReader();
            return reader.Read() ? ProjectMapper.MapProject(reader) : null;
        }
        catch (SqlException ex)
        {
            clsLog.LogError(nameof(ProjectRepository), nameof(GetProjectById), ex);
            throw;
        }
        catch (Exception ex)
        {
            clsLog.LogError(nameof(ProjectRepository), nameof(GetProjectById), ex);
            throw;
        }
    }

    // ======================== [ GET ALL PROJECTS ] ========================
    public static List<ProjectDetails> GetAllProjects()
    {
        var list = new List<ProjectDetails>();

        try
        {
            using var conn = new SqlConnection(ConnectionString);
            using var cmd = new SqlCommand("sp_GetAllProjects", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            conn.Open();

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                list.Add(ProjectMapper.MapProjectDetails(reader));
        }
        catch (SqlException ex)
        {
            clsLog.LogError(nameof(ProjectRepository), nameof(GetAllProjects), ex);
            throw;
        }
        catch (Exception ex)
        {
            clsLog.LogError(nameof(ProjectRepository), nameof(GetAllProjects), ex);
            throw;
        }

        return list;
    }

    // ======================== [ GET PROJECTS BY CATEGORY ] ========================
    public static List<ProjectDetails> GetProjectsByCategory(int categoryId)
    {
        var list = new List<ProjectDetails>();

        try
        {
            using var conn = new SqlConnection(ConnectionString);
            using var cmd = new SqlCommand("sp_GetProjectsByCategory", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@CategoryId", categoryId);
            conn.Open();

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                list.Add(ProjectMapper.MapProjectDetails(reader));
        }
        catch (SqlException ex)
        {
            clsLog.LogError(nameof(ProjectRepository), nameof(GetProjectsByCategory), ex);
            throw;
        }
        catch (Exception ex)
        {
            clsLog.LogError(nameof(ProjectRepository), nameof(GetProjectsByCategory), ex);
            throw;
        }

        return list;
    }

    // ======================== [ GET PROJECTS BY STATUS ] ========================
    public static List<ProjectDetails> GetProjectsByStatus(string status)
    {
        var list = new List<ProjectDetails>();

        try
        {
            using var conn = new SqlConnection(ConnectionString);
            using var cmd = new SqlCommand("sp_GetProjectsByStatus", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@Status", status);
            conn.Open();

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                list.Add(ProjectMapper.MapProjectDetails(reader));
        }
        catch (SqlException ex)
        {
            clsLog.LogError(nameof(ProjectRepository), nameof(GetProjectsByStatus), ex);
            throw;
        }
        catch (Exception ex)
        {
            clsLog.LogError(nameof(ProjectRepository), nameof(GetProjectsByStatus), ex);
            throw;
        }

        return list;
    }

    // ======================== [ IS PROJECT NAME TAKEN ] ========================
    public static bool IsProjectNameTaken(string title)
    {
        try
        {
            using var conn = new SqlConnection(ConnectionString);
            using var cmd = new SqlCommand("sp_IsProjectNameTaken", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@Title", title);
            conn.Open();

            using var reader = cmd.ExecuteReader();
            return reader.Read() && reader.GetInt32(reader.GetOrdinal("IsTaken")) == 1;
        }
        catch (SqlException ex)
        {
            clsLog.LogError(nameof(ProjectRepository), nameof(IsProjectNameTaken), ex);
            throw;
        }
        catch (Exception ex)
        {
            clsLog.LogError(nameof(ProjectRepository), nameof(IsProjectNameTaken), ex);
            throw;
        }
    }

    // ======================== [ IS PROJECT NAME TAKEN BY OTHER ] ========================
    public static bool IsProjectNameTakenByOther(int projectId, string title)
    {
        try
        {
            using var conn = new SqlConnection(ConnectionString);
            using var cmd = new SqlCommand("sp_IsProjectNameTakenByOther", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@ProjectId", projectId);
            cmd.Parameters.AddWithValue("@Title", title);
            conn.Open();

            using var reader = cmd.ExecuteReader();
            return reader.Read() && reader.GetInt32(reader.GetOrdinal("IsTaken")) == 1;
        }
        catch (SqlException ex)
        {
            clsLog.LogError(nameof(ProjectRepository), nameof(IsProjectNameTakenByOther), ex);
            throw;
        }
        catch (Exception ex)
        {
            clsLog.LogError(nameof(ProjectRepository), nameof(IsProjectNameTakenByOther), ex);
            throw;
        }
    }
}