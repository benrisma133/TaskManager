using System.Text;

namespace Repository.Loggers
{
    public class clsLog
    {
        private static string _BuildLog(string className, string methodName, Exception ex)
        {
            return new StringBuilder()
                .Append($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC] ")
                .Append($"[{className}.{methodName}] ")
                .Append($"{ex.GetType().Name}: {ex.Message}")
                .ToString();
        }

        public static void LogError(string className, string methodName, Exception ex)
        {
            string log = _BuildLog(className, methodName, ex);
            Console.Error.WriteLine(log);
        }

        public static void LogError(string methodName, Exception ex)
        {
            Console.Error.WriteLine(
                $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC] " +
                $"[{methodName}] " +
                $"{ex.GetType().Name}: {ex.Message}");
        }
    }
}
