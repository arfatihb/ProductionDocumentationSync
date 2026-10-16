using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ClosedXML.Excel;

namespace ProductionDocumentationSync
{
    /// <summary>Tek bir günlük satırı. Hem canlı günlük hem de dosyadan okunan geçmiş için kullanılır.</summary>
    public sealed class LogEntry
    {
        public long Seq;
        public DateTime Time;
        public string Level;     // INFO / OK / WARN / FAIL
        public string Step;      // SESSION, LIST, COPY, EXCEL, PDM, DONE, CRASH ...
        public string Message;
        public string Project;
        public string Sub;
        public string Code;      // ürün kodu
        public string Op;        // operasyon / kaynak kodu
        public string Path;      // hedef dosya/klasör ya da ek bilgi

        public bool IsProblem
        {
            get { return Level == "FAIL" || Level == "WARN"; }
        }

        public string[] ToFields()
        {
            return new[]
            {
                Time.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                Level ?? "", Step ?? "", Message ?? "",
                Project ?? "", Sub ?? "", Code ?? "", Op ?? "", Path ?? ""
            };
        }
    }

    /// <summary>
    /// Bir çalışma (oturum) günlüğü. Her satır anında, sekmeyle ayrılmış metin dosyasına yazılır (çökse bile kalır).
    /// Oturum bitince aynı içerik tek seferde Run_*.xlsx olarak da yazılır.
    /// Kullanıcı arayüzü EntryAdded olayı ile canlı beslenir.
    /// </summary>
    public sealed class RunLogger : IDisposable
    {
        public const string Header = "Time\tLevel\tStep\tMessage\tProject\tSub\tBase\tOp\tExtra";

        /// <summary>Uygulama genelinde aktif günlük (çökme yakalayıcı buraya yazar).</summary>
        public static RunLogger Current;

        public string LogDir { get; private set; }
        public string TxtPath { get; private set; }
        public string XlsxPath { get; private set; }
        public string Mode { get; private set; }
        public DateTime Started { get; private set; }
        /// <summary>İstenen günlük klasörü kullanılamadıysa neden; null = sorun yok.</summary>
        public string FallbackReason { get; private set; }

        public int OkCopies { get; private set; }
        public int FailCopies { get; private set; }
        public int ExcelCount { get; private set; }
        public int Fails { get; private set; }
        public int Warns { get; private set; }

        private readonly List<LogEntry> _entries = new List<LogEntry>();
        private readonly object _lock = new object();
        private long _seq;
        private bool _finished;

        /// <summary>Her yeni satırda tetiklenir; çağıran iş parçacığında çalışır (UI'ya marshal etmek çağıranın işi).</summary>
        public event Action<LogEntry> EntryAdded;

        public RunLogger(string logDir, string mode)
        {
            Mode = mode ?? "MANUAL";
            Started = DateTime.Now;

            string stamp = Started.ToString("yyyyMMdd_HHmmss") + "_" + Environment.UserName;
            string fallbackDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                                              "ProductionDocumentationSync", "Logs");

            // Önce istenen klasör; klasör ya da dosya oluşturulamazsa (erişim, çok uzun yol, sürücü yok...) yerel klasör.
            string err;
            if (!TryUseDir(logDir, stamp, out err))
            {
                FallbackReason = "Günlük klasörü kullanılamadı (" + logDir + ": " + err + "); yerel klasöre yazılıyor: " + fallbackDir;
                if (!TryUseDir(fallbackDir, stamp, out err))
                    throw new IOException("Günlük dosyası hiçbir yere yazılamıyor: " + err);
            }
        }

        private bool TryUseDir(string dir, string stamp, out string error)
        {
            error = null;
            try
            {
                Directory.CreateDirectory(dir);
                string txt = Path.Combine(dir, "Run_" + stamp + ".log");
                using (var fs = new FileStream(txt, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                using (var sw = new StreamWriter(fs, new UTF8Encoding(true)))
                {
                    sw.WriteLine(Header);
                }
                LogDir = dir;
                TxtPath = txt;
                XlsxPath = Path.Combine(dir, "Run_" + stamp + ".xlsx");
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        public LogEntry Log(string level, string step, string message,
                            string project = null, string sub = null, string code = null,
                            string op = null, string path = null)
        {
            var e = new LogEntry
            {
                Time = DateTime.Now,
                Level = level ?? "INFO",
                Step = step ?? "",
                Message = message ?? "",
                Project = project,
                Sub = sub,
                Code = code,
                Op = op,
                Path = path
            };

            lock (_lock)
            {
                e.Seq = ++_seq;
                _entries.Add(e);
                if (e.Level == "FAIL") Fails++;
                else if (e.Level == "WARN") Warns++;
                if (e.Step == "COPY")
                {
                    if (e.Level == "OK") OkCopies++;
                    else if (e.Level == "FAIL") FailCopies++;
                }
                if ((e.Step == "EXCEL" || e.Step == "EXCEL_OP") && e.Level == "OK") ExcelCount++;

                AppendLine(string.Join("\t", e.ToFields().Select(Escape)));
            }

            var h = EntryAdded;
            if (h != null)
            {
                try { h(e); } catch { /* UI kapanmış olabilir */ }
            }
            return e;
        }

        public List<LogEntry> Snapshot()
        {
            lock (_lock) return new List<LogEntry>(_entries);
        }

        public int Count
        {
            get { lock (_lock) return _entries.Count; }
        }

        /// <summary>Oturumu kapatır: "Finished" satırı + xlsx. Birden fazla çağrılabilir; yalnız ilki iş yapar.</summary>
        public void Finish(string summary)
        {
            lock (_lock)
            {
                if (_finished) return;
                _finished = true;
            }
            Log("INFO", "SESSION", "Finished", path: summary);
            WriteXlsx();
        }

        public void Dispose()
        {
            Finish(null);
        }

        private void WriteXlsx()
        {
            try
            {
                List<LogEntry> rows = Snapshot();
                using (var wb = new XLWorkbook())
                {
                    var ws = wb.Worksheets.Add("Log");
                    string[] headers = Header.Split('\t');
                    for (int c = 0; c < headers.Length; c++) ws.Cell(1, c + 1).Value = headers[c];
                    ws.Range(1, 1, 1, headers.Length).Style.Font.Bold = true;

                    int r = 2;
                    foreach (var e in rows)
                    {
                        ws.Cell(r, 1).Value = e.Time;
                        ws.Cell(r, 1).Style.DateFormat.Format = "yyyy-mm-dd hh:mm:ss";
                        ws.Cell(r, 2).Value = e.Level;
                        ws.Cell(r, 3).Value = e.Step;
                        ws.Cell(r, 4).Value = e.Message ?? "";
                        ws.Cell(r, 5).Value = e.Project ?? "";
                        ws.Cell(r, 6).Value = e.Sub ?? "";
                        ws.Cell(r, 7).Value = e.Code ?? "";
                        ws.Cell(r, 8).Value = e.Op ?? "";
                        ws.Cell(r, 9).Value = e.Path ?? "";
                        if (e.Level == "FAIL") ws.Row(r).Style.Font.FontColor = XLColor.Red;
                        else if (e.Level == "WARN") ws.Row(r).Style.Font.FontColor = XLColor.DarkOrange;
                        r++;
                    }
                    ws.SheetView.FreezeRows(1);
                    ws.Columns().AdjustToContents(1, Math.Min(r, 500));
                    wb.SaveAs(XlsxPath);
                }
            }
            catch { /* xlsx yazılamadıysa .log zaten diskte */ }
        }

        private static string Escape(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Replace("\t", " ").Replace("\r", " ").Replace("\n", " ");
        }

        private void AppendLine(string line)
        {
            try
            {
                using (var fs = new FileStream(TxtPath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                using (var sw = new StreamWriter(fs, new UTF8Encoding(true)))
                {
                    sw.WriteLine(line);
                    sw.Flush();
                    fs.Flush(true);
                }
            }
            catch { /* günlük için çökme */ }
        }
    }
}
