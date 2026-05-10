using Repository.Data;
using System.Configuration;
using System.Data;
using System.Windows;

namespace TaskManagerUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            Task.Run(() =>
            {
                try
                {
                    using var conn = DatabaseHelper.GetConnection();
                    conn.Open(); // warm up connection pool
                }
                catch { }
            });
        }
    }

}
