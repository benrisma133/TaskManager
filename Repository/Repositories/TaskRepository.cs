using Microsoft.Data.SqlClient;
using Repository.Data;
using Repository.Loggers;
using Repository.Mappers;
using Repository.Models;
using System.Data;

namespace Repository.Repositories;

public static class TaskRepository
{
    private static string ConnectionString =>
        DatabaseHelper.ConnectionString;

    // ======================== [ ADD TASK ] ========================
    public static int AddTask(TaskItem task)
    {
        try
        {
            using var conn = new SqlConnection(ConnectionString);
            using var cmd = new SqlCommand("sp_AddTask", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@Title", task.Title);
            cmd.Parameters.AddWithValue("@Description", (object?)task.Description ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ProjectId", task.ProjectID);
            cmd.Parameters.AddWithValue("@Priority", task.Priority);
            cmd.Parameters.AddWithValue("@DueDate", (object?)task.DueDate?.ToDateTime(TimeOnly.MinValue) ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@EstimatedMinutes", (object?)task.EstimatedMinutes ?? DBNull.Value);

            conn.Open();

            var result = cmd.ExecuteScalar();
            return result is not null ? Convert.ToInt32(result) : -1;
        }
        catch (SqlException ex)
        {
            clsLog.LogError(nameof(TaskRepository), nameof(AddTask), ex);
            throw;
        }
        catch (Exception ex)
        {
            clsLog.LogError(nameof(TaskRepository), nameof(AddTask), ex);
            throw;
        }
    }

    // ======================== [ UPDATE TASK ] ========================
    public static bool UpdateTask(TaskItem task)
    {
        try
        {
            using var conn = new SqlConnection(ConnectionString);
            using var cmd = new SqlCommand("sp_UpdateTask", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@TaskId", task.TaskID);
            cmd.Parameters.AddWithValue("@Title", task.Title);
            cmd.Parameters.AddWithValue("@Description", (object?)task.Description ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Priority", task.Priority);
            cmd.Parameters.AddWithValue("@DueDate", (object?)task.DueDate?.ToDateTime(TimeOnly.MinValue) ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@EstimatedMinutes", (object?)task.EstimatedMinutes ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Status", task.Status);

            conn.Open();
            return cmd.ExecuteNonQuery() > 0;
        }
        catch (SqlException ex)
        {
            clsLog.LogError(nameof(TaskRepository), nameof(UpdateTask), ex);
            throw;
        }
        catch (Exception ex)
        {
            clsLog.LogError(nameof(TaskRepository), nameof(UpdateTask), ex);
            throw;
        }
    }

    // ======================== [ DELETE TASK (HARD) ] ========================
    public static void DeleteTask(int taskId)
    {
        try
        {
            using var conn = new SqlConnection(ConnectionString);
            using var cmd = new SqlCommand("sp_DeleteTask", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@TaskId", taskId);

            conn.Open();
            cmd.ExecuteNonQuery();
        }
        catch (SqlException ex)
        {
            clsLog.LogError(nameof(TaskRepository), nameof(DeleteTask), ex);
            throw;
        }
        catch (Exception ex)
        {
            clsLog.LogError(nameof(TaskRepository), nameof(DeleteTask), ex);
            throw;
        }
    }

    // ======================== [ SOFT DELETE TASK ] ========================
    public static string SoftDeleteTask(int taskId)
    {
        try
        {
            using var conn = new SqlConnection(ConnectionString);
            using var cmd = new SqlCommand("sp_SoftDeleteTask", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@TaskId", taskId);
            conn.Open();

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
                return reader.GetString(reader.GetOrdinal("Result"));

            return "UNKNOWN";
        }
        catch (SqlException ex)
        {
            clsLog.LogError(nameof(TaskRepository), nameof(SoftDeleteTask), ex);
            throw;
        }
        catch (Exception ex)
        {
            clsLog.LogError(nameof(TaskRepository), nameof(SoftDeleteTask), ex);
            throw;
        }
    }

    // ======================== [ GET TASK BY ID ] ========================
    public static TaskItem? GetTaskById(int taskId)
    {
        try
        {
            using var conn = new SqlConnection(ConnectionString);
            using var cmd = new SqlCommand("sp_GetTaskById", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@TaskId", taskId);
            conn.Open();

            using var reader = cmd.ExecuteReader();
            return reader.Read() ? TaskMapper.MapTaskItem(reader) : null;
        }
        catch (SqlException ex)
        {
            clsLog.LogError(nameof(TaskRepository), nameof(GetTaskById), ex);
            throw;
        }
        catch (Exception ex)
        {
            clsLog.LogError(nameof(TaskRepository), nameof(GetTaskById), ex);
            throw;
        }
    }

    // ======================== [ GET ALL TASKS PAGED ] ========================
    public static (List<TaskItemDetails> tasks, int totalCount) GetAllTasks(
            int pageNumber = 1, int pageSize = 9,
            string? search = null, string? priority = null, string? status = null)
    {
        var list = new List<TaskItemDetails>();
        int totalCount = 0;

        try
        {
            using var conn = new SqlConnection(ConnectionString);
            using var cmd = new SqlCommand("sp_GetAllTasks", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@PageNumber", pageNumber);
            cmd.Parameters.AddWithValue("@PageSize", pageSize);
            cmd.Parameters.AddWithValue("@Search", (object?)search ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Priority", (object?)priority ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Status", (object?)status ?? DBNull.Value);

            conn.Open();

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                list.Add(TaskMapper.MapTaskItemDetails(reader));

            if (reader.NextResult() && reader.Read())
                totalCount = reader.GetInt32(reader.GetOrdinal("TotalCount"));
        }
        catch (SqlException ex)
        {
            clsLog.LogError(nameof(TaskRepository), nameof(GetAllTasks), ex);
            throw;
        }
        catch (Exception ex)
        {
            clsLog.LogError(nameof(TaskRepository), nameof(GetAllTasks), ex);
            throw;
        }

        return (list, totalCount);
    }

    // ======================== [ GET TASKS BY PROJECT ] ========================
    public static List<TaskItemDetails> GetTasksByProject(int projectId)
    {
        var list = new List<TaskItemDetails>();

        try
        {
            using var conn = new SqlConnection(ConnectionString);
            using var cmd = new SqlCommand("sp_GetTasksByProject", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@ProjectId", projectId);
            conn.Open();

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                list.Add(TaskMapper.MapTaskItemDetails(reader));
        }
        catch (SqlException ex)
        {
            clsLog.LogError(nameof(TaskRepository), nameof(GetTasksByProject), ex);
            throw;
        }
        catch (Exception ex)
        {
            clsLog.LogError(nameof(TaskRepository), nameof(GetTasksByProject), ex);
            throw;
        }

        return list;
    }

    // ======================== [ IS TASK NAME TAKEN ] ========================
    public static bool IsTaskNameTaken(int projectId, string title)
    {
        try
        {
            using var conn = new SqlConnection(ConnectionString);
            using var cmd = new SqlCommand("sp_IsTaskNameTaken", conn)
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
            clsLog.LogError(nameof(TaskRepository), nameof(IsTaskNameTaken), ex);
            throw;
        }
        catch (Exception ex)
        {
            clsLog.LogError(nameof(TaskRepository), nameof(IsTaskNameTaken), ex);
            throw;
        }
    }

    // ======================== [ IS TASK NAME TAKEN BY OTHER ] ========================
    public static bool IsTaskNameTakenByOther(int taskId, int projectId, string title)
    {
        try
        {
            using var conn = new SqlConnection(ConnectionString);
            using var cmd = new SqlCommand("sp_IsTaskNameTakenByOther", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@TaskId", taskId);
            cmd.Parameters.AddWithValue("@ProjectId", projectId);
            cmd.Parameters.AddWithValue("@Title", title);
            conn.Open();

            using var reader = cmd.ExecuteReader();
            return reader.Read() && reader.GetInt32(reader.GetOrdinal("IsTaken")) == 1;
        }
        catch (SqlException ex)
        {
            clsLog.LogError(nameof(TaskRepository), nameof(IsTaskNameTakenByOther), ex);
            throw;
        }
        catch (Exception ex)
        {
            clsLog.LogError(nameof(TaskRepository), nameof(IsTaskNameTakenByOther), ex);
            throw;
        }
    }

}