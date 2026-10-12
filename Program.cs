using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace ProductionDocumentationSync
{
    static class Program
    {
        private const string CRASH_LOG_DIR = @"\\192.168.100.66\GebzeUretim\_Raporlar";

        [STAThread]
        static void Main()
        {
            // /auto modunda dialog patlamasın, log'a yazıp sessizce çıksın.
            string[] args = Environment.GetCommandLineArgs();
            bool isAuto = (args.Length > 1 && args[1].Equals("/auto", StringComparison.OrdinalIgnoreCase));

            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += (s, e) =>
            {
                WriteCrashLog(e.Exception, isAuto ? "AUTO" : "MANUAL");
                if (isAuto) Environment.Exit(1);
            };
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                WriteCrashLog(e.ExceptionObject as Exception, isAuto ? "AUTO" : "MANUAL");
                if (isAuto) Environment.Exit(1);
            };

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }

        private static void WriteCrashLog(Exception ex, string mode)
        {
            if (ex == null) return;
            try
            {
                Directory.CreateDirectory(CRASH_LOG_DIR);
                string path = Path.Combine(CRASH_LOG_DIR,
                    $"Crash_{DateTime.Now:yyyyMMdd_HHmmss}_{Environment.UserName}.log");
                File.AppendAllText(path,
                    $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Mode={mode}{Environment.NewLine}{ex}{Environment.NewLine}");
            }
            catch { /* log için crash etme */ }
        }
    }
}
