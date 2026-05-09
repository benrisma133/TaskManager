using Microsoft.Data.SqlClient;
using Repository.Loggers;
using Repository.Mappers;
using Repository.Models;
using System.Data;
using Repository.Data;

namespace Repository.Repositories;

public static class CategoryRepository
{
    private static string ConnectionString =>
        DatabaseHelper.ConnectionString;

    // ======================== [ GET ALL CATEGORIES ] ========================
    public static List<Category> GetAllCategories()
    {
        var list = new List<Category>();

        try
        {
            using var conn = new SqlConnection(ConnectionString);
            using var cmd = new SqlCommand("sp_GetAllCategories", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            conn.Open();

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                list.Add(CategoryMapper.Map(reader));
        }
        catch (SqlException ex)
        {
            clsLog.LogError(nameof(CategoryRepository), nameof(GetAllCategories), ex);
            throw;
        }
        catch (Exception ex)
        {
            clsLog.LogError(nameof(CategoryRepository), nameof(GetAllCategories), ex);
            throw;
        }

        return list;
    }

    // ======================== [ GET CATEGORY BY ID ] ========================
    public static Category? GetCategoryById(int categoryId)
    {
        try
        {
            using var conn = new SqlConnection(ConnectionString);
            using var cmd = new SqlCommand("sp_GetCategoryById", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@CategoryId", categoryId);
            conn.Open();

            using var reader = cmd.ExecuteReader();
            return reader.Read() ? CategoryMapper.Map(reader) : null;
        }
        catch (SqlException ex)
        {
            clsLog.LogError(nameof(CategoryRepository), nameof(GetCategoryById), ex);
            throw;
        }
        catch (Exception ex)
        {
            clsLog.LogError(nameof(CategoryRepository), nameof(GetCategoryById), ex);
            throw;
        }
    }

    // ======================== [ ADD CATEGORY ] ========================
    public static int AddCategory(Category category)
    {
        try
        {
            using var conn = new SqlConnection(ConnectionString);
            using var cmd = new SqlCommand("sp_AddCategory", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@Name", category.Name);
            cmd.Parameters.AddWithValue("@Color", category.Color);
            cmd.Parameters.AddWithValue("@Icon", category.Icon);
            cmd.Parameters.AddWithValue("@Type", category.Type);

            conn.Open();

            var result = cmd.ExecuteScalar();
            return result is not null ? Convert.ToInt32(result) : -1;
        }
        catch (SqlException ex)
        {
            clsLog.LogError(nameof(CategoryRepository), nameof(AddCategory), ex);
            throw;
        }
        catch (Exception ex)
        {
            clsLog.LogError(nameof(CategoryRepository), nameof(AddCategory), ex);
            throw;
        }
    }

    // ======================== [ UPDATE CATEGORY ] ========================
    public static bool UpdateCategory(Category category)
    {
        try
        {
            using var conn = new SqlConnection(ConnectionString);
            using var cmd = new SqlCommand("sp_UpdateCategory", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@CategoryId", category.CategoryId);
            cmd.Parameters.AddWithValue("@Name", category.Name);
            cmd.Parameters.AddWithValue("@Color", category.Color);
            cmd.Parameters.AddWithValue("@Icon", category.Icon);
            cmd.Parameters.AddWithValue("@Type", category.Type);

            conn.Open();
            return cmd.ExecuteNonQuery() > 0;
        }
        catch (SqlException ex)
        {
            clsLog.LogError(nameof(CategoryRepository), nameof(UpdateCategory), ex);
            throw;
        }
        catch (Exception ex)
        {
            clsLog.LogError(nameof(CategoryRepository), nameof(UpdateCategory), ex);
            throw;
        }
    }

    // ======================== [ DELETE CATEGORY ] ========================
    public static string DeleteCategory(int categoryId)
    {
        try
        {
            using var conn = new SqlConnection(ConnectionString);
            using var cmd = new SqlCommand("sp_DeleteCategory", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@CategoryId", categoryId);
            conn.Open();

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
                return reader.GetString(reader.GetOrdinal("Result"));

            return "UNKNOWN";
        }
        catch (SqlException ex)
        {
            clsLog.LogError(nameof(CategoryRepository), nameof(DeleteCategory), ex);
            throw;
        }
        catch (Exception ex)
        {
            clsLog.LogError(nameof(CategoryRepository), nameof(DeleteCategory), ex);
            throw;
        }
    }

    // ======================== [ IS NAME TAKEN ] ========================
    public static bool IsCategoryNameTaken(string name)
    {
        try
        {
            using var conn = new SqlConnection(ConnectionString);
            using var cmd = new SqlCommand("sp_IsCategoryNameTaken", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@Name", name);
            conn.Open();

            using var reader = cmd.ExecuteReader();
            return reader.Read() && reader.GetInt32(reader.GetOrdinal("IsTaken")) == 1;
        }
        catch (SqlException ex)
        {
            clsLog.LogError(nameof(CategoryRepository), nameof(IsCategoryNameTaken), ex);
            throw;
        }
        catch (Exception ex)
        {
            clsLog.LogError(nameof(CategoryRepository), nameof(IsCategoryNameTaken), ex);
            throw;
        }
    }

    // ======================== [ IS NAME TAKEN BY OTHER ] ========================
    public static bool IsCategoryNameTakenByOther(int categoryId, string name)
    {
        try
        {
            using var conn = new SqlConnection(ConnectionString);
            using var cmd = new SqlCommand("sp_IsCategoryNameTakenByOther", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@CategoryId", categoryId);
            cmd.Parameters.AddWithValue("@Name", name);
            conn.Open();

            using var reader = cmd.ExecuteReader();
            return reader.Read() && reader.GetInt32(reader.GetOrdinal("IsTaken")) == 1;
        }
        catch (SqlException ex)
        {
            clsLog.LogError(nameof(CategoryRepository), nameof(IsCategoryNameTakenByOther), ex);
            throw;
        }
        catch (Exception ex)
        {
            clsLog.LogError(nameof(CategoryRepository), nameof(IsCategoryNameTakenByOther), ex);
            throw;
        }
    }
}