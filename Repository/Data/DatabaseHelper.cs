using Microsoft.Data.SqlClient;

namespace Repository.Data;

public class DatabaseHelper
{
    private static string Server => Environment.GetEnvironmentVariable("DB_SERVER") ?? @"localhost\SQLEXPRESS01";
    private static string DbName => Environment.GetEnvironmentVariable("DB_NAME") ?? "TaskFlowDb";

    private static string MasterConnectionString =>
        $"Server={Server};Database=master;Trusted_Connection=True;TrustServerCertificate=True;";

    public static string ConnectionString =>
        $"Server={Server};Database={DbName};Trusted_Connection=True;TrustServerCertificate=True;";

    public static SqlConnection GetConnection() => new SqlConnection(ConnectionString);
}