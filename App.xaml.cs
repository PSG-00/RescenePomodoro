using System;
using System.IO;
using System.Windows;

namespace radiant_noether
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            AppDomain.CurrentDomain.UnhandledException += (s, args) =>
            {
                LogException("AppDomain.UnhandledException", args.ExceptionObject as Exception);
            };

            DispatcherUnhandledException += (s, args) =>
            {
                LogException("DispatcherUnhandledException", args.Exception);
                args.Handled = true;
            };
        }

        private static void LogException(string source, Exception? ex)
        {
            try
            {
                string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "crash.log");
                string content = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{source}]\n{ex?.ToString()}\n\n";
                File.AppendAllText(logPath, content);
            }
            catch { }
        }
    }
}
