using System;
using System.Diagnostics;
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
        private static Mutex _singleInstance;

        [STAThread]
        static int Main(string[] args)
        {
            bool isAuto = args.Any(a => a.Equals("/auto", StringComparison.OrdinalIgnoreCase)
                                     || a.Equals("-auto", StringComparison.OrdinalIgnoreCase)
                                     || a.Equals("--auto", StringComparison.OrdinalIgnoreCase));
            string mode = isAuto ? "AUTO" : "MANUAL";

            bool createdNew;
            _singleInstance = new Mutex(true, @"Local\ProductionDocumentationSync_SingleInstance", out createdNew);
            if (!createdNew)
            {
                IntPtr other = FindOtherInstanceWindow();
                if (isAuto)
                {
                    // Zamanlanmış çalışma: pencere açık. Günlüğe "atlandı" yaz, açık pencereye haber ver.
                    try
                    {
                        var lg = new RunLogger(AppConfig.LogDir, "AUTO");
                        lg.Log("INFO", "SESSION", "Started", path: "AUTO");
                        lg.Log("WARN", "SKIP", "Zamanlanmış çalışma atlandı: uygulama penceresi zaten açıktı (ExitCode=4)");
                        lg.Finish("Atlandı | ExitCode=4");
                    }
                    catch { }
                    if (other != IntPtr.Zero)
                        NativeMethods.PostMessage(other, NativeMethods.WM_APP_NOTIFY, (IntPtr)1, IntPtr.Zero);
                    return 4;
                }

                // Manuel: açık pencereyi öne getir
                if (other != IntPtr.Zero)
                {
                    if (NativeMethods.IsIconic(other)) NativeMethods.ShowWindow(other, NativeMethods.SW_RESTORE);
                    NativeMethods.SetForegroundWindow(other);
                    NativeMethods.PostMessage(other, NativeMethods.WM_APP_NOTIFY, (IntPtr)2, IntPtr.Zero);
                }
                else
                {
                    MessageBox.Show("Uygulama zaten çalışıyor.", "Üretim Doküman Senkronizasyonu",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                return 4;
            }

            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += (s, e) => HandleFatal(e.Exception, isAuto);
            AppDomain.CurrentDomain.UnhandledException += (s, e) => HandleFatal(e.ExceptionObject as Exception, isAuto);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1(isAuto));

            ReleaseSingleInstance();
            return Environment.ExitCode;
        }

        /// <summary>
        /// Tek örnek kilidini bırakır. Pencere kapanmadan hemen önce görev elle başlatılırken kullanılır;
        /// böylece /auto örneği "zaten çalışıyor" (4) ile atlanmaz.
        /// </summary>
        public static void ReleaseSingleInstance()
        {
            var m = _singleInstance;
            _singleInstance = null;
            if (m == null) return;
            try { m.ReleaseMutex(); } catch { }
            try { m.Dispose(); } catch { }
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

        /// <summary>Çalışan diğer örneğin ana pencere tutamacı; yoksa IntPtr.Zero.</summary>
        private static IntPtr FindOtherInstanceWindow()
        {
            try
            {
                var me = Process.GetCurrentProcess();
                foreach (var p in Process.GetProcessesByName(me.ProcessName))
                {
                    try
                    {
                        if (p.Id != me.Id && p.MainWindowHandle != IntPtr.Zero) return p.MainWindowHandle;
                    }
                    catch { }
                }
            }
            catch { }
            return IntPtr.Zero;
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
