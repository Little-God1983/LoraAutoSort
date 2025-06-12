using System.Windows;
using Serilog;

namespace UI.LoraSort
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Configure Serilog directly in code
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File("Logs/LoraSort-log.txt",       // Writes to rolling file
                    rollingInterval: RollingInterval.Day)
                .CreateLogger();

            // Example usage:
            Log.Information("Application starting up (via code config)");
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Log.CloseAndFlush(); // Ensure logs are flushed on exit
            base.OnExit(e);
        }
    }

}
