using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace ProductionDocumentationSync
{
    /// <summary>
    /// Çıkış kodları (Görev Zamanlayıcı "Son Çalıştırma Sonucu"):
    ///   0 = başarılı, 1 = çökme, 2 = kopyalama hatalarıyla tamamlandı,
    ///   3 = zaman aşımı, 4 = zaten çalışıyor, 5 = yapılandırma hatası
    /// </summary>
    static class Program
    {
        [STAThread]
        static int Main(string[] args)
        {
            bool isAuto = args.Any(a => a.Equals("/auto", StringComparison.OrdinalIgnoreCase)
                                     || a.Equals("-auto", StringComparison.OrdinalIgnoreCase)
                                     || a.Equals("--auto", StringComparison.OrdinalIgnoreCase));
            string mode = isAuto ? "AUTO" : "MANUAL";

            bool createdNew;
            using (var mutex = new Mutex(true, @"Local\ProductionDocumentationSync_SingleInstance", out createdNew))
            {
                if (!createdNew)
                {
                    WriteCrashFile(null, mode, "Uygulama zaten çalışıyor; bu örnek kapatıldı (ExitCode=4).");
                    if (!isAuto)
                        MessageBox.Show("Uygulama zaten çalışıyor.", "Üretim Doküman Senkronizasyonu",
                                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return 4;
                }

                Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
                Application.ThreadException += (s, e) => HandleFatal(e.Exception, isAuto);
                AppDomain.CurrentDomain.UnhandledException += (s, e) => HandleFatal(e.ExceptionObject as Exception, isAuto);

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Form1(isAuto));

                GC.KeepAlive(mutex);
                return Environment.ExitCode;
            }
        }

        private static void HandleFatal(Exception ex, bool isAuto)
        {
            if (ex == null) return;
            var logger = RunLogger.Current;
            try { if (logger != null) logger.Log("FAIL", "CRASH", ex.ToString()); } catch { }
            WriteCrashFile(ex, isAuto ? "AUTO" : "MANUAL", null);

            if (isAuto)
            {
                try { if (logger != null) logger.Finish("CRASH | ExitCode=1"); } catch { }
                Environment.Exit(1);
            }

            MessageBox.Show("Beklenmeyen hata:\n" + ex.Message +
                            "\n\nAyrıntı: " + (logger != null ? logger.TxtPath : AppConfig.LogDir),
                            "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        /// <summary>Günlük nesnesi daha oluşmamışken (ya da çökerken) bağımsız bir dosyaya yazar.</summary>
        private static void WriteCrashFile(Exception ex, string mode, string note)
        {
            try
            {
                string dir = AppConfig.LogDir;
                try { Directory.CreateDirectory(dir); }
                catch
                {
                    dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                                       "ProductionDocumentationSync", "Logs");
                    Directory.CreateDirectory(dir);
                }
                string path = Path.Combine(dir, "Crash_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + "_" + Environment.UserName + ".log");
                File.AppendAllText(path,
                    "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] Mode=" + mode + Environment.NewLine +
                    (note ?? "") + (ex != null ? ex.ToString() : "") + Environment.NewLine);
            }
            catch { /* günlük için çökme */ }
        }
    }
}
