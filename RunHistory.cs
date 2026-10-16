using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace ProductionDocumentationSync
{
    /// <summary>Bir geçmiş çalışmanın özeti (Run_*.log dosyasından türetilir).</summary>
    public sealed class RunSummary
    {
        public string FilePath;
        public DateTime Started;
        public DateTime? Finished;
        public string Mode = "";
        public string User = "";
        public int Groups = -1;      // -1 = listeleme yapılmadı
        public int CopiedOk;
        public int CopyFail;
        public int Excel;
        public int Fails;
        public int Warns;
        public bool Completed;
        public bool Crashed;
        public bool TimedOut;
        public bool HasCopy;

        public TimeSpan? Duration
        {
            get { return Finished.HasValue ? Finished.Value - Started : (TimeSpan?)null; }
        }

        public string Result
        {
            get
            {
                if (Crashed) return "Çöktü";
                if (TimedOut) return "Zaman aşımı";
                if (!Completed) return "Yarım kaldı";
                if (CopyFail > 0 || Fails > 0) return "Hatalarla tamamlandı";
                if (!HasCopy) return Groups >= 0 ? "Sadece listeleme" : "İşlem yapılmadı";
                return "Tamamlandı";
            }
        }

        public bool IsProblem
        {
            get { return Crashed || TimedOut || !Completed || CopyFail > 0 || Fails > 0; }
        }
    }

    /// <summary>Run_*.log dosyalarını okur ve özetler.</summary>
    public static class RunHistory
    {
        private const string TimeFormat = "yyyy-MM-dd HH:mm:ss.fff";

        public static List<RunSummary> LoadSummaries(string logDir, int max = 100)
        {
            var result = new List<RunSummary>();
            if (string.IsNullOrEmpty(logDir) || !Directory.Exists(logDir)) return result;

            var files = Directory.EnumerateFiles(logDir, "Run_*.log")
                                 .OrderByDescending(f => Path.GetFileName(f), StringComparer.OrdinalIgnoreCase)
                                 .Take(max);

            foreach (var f in files)
            {
                try { result.Add(Summarize(f, ReadEntries(f))); }
                catch { /* bozuk dosya: atla */ }
            }
            return result;
        }

        public static List<LogEntry> ReadEntries(string path)
        {
            var list = new List<LogEntry>();
            long seq = 0;
            // Yazan taraf FileShare.ReadWrite ile açıyor; biz de paylaşımlı okuyoruz (canlı dosya olabilir).
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var sr = new StreamReader(fs, true))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    if (line.Length == 0 || line.StartsWith("Time\t", StringComparison.Ordinal)) continue;
                    var e = ParseLine(line);
                    if (e == null) continue;
                    e.Seq = ++seq;
                    list.Add(e);
                }
            }
            return list;
        }

        public static LogEntry ParseLine(string line)
        {
            var p = line.Split('\t');
            if (p.Length < 4) return null;
            DateTime t;
            if (!DateTime.TryParseExact(p[0], TimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out t) &&
                !DateTime.TryParse(p[0], CultureInfo.InvariantCulture, DateTimeStyles.None, out t))
                return null;

            Func<int, string> at = i => i < p.Length ? p[i] : "";
            return new LogEntry
            {
                Time = t,
                Level = at(1),
                Step = at(2),
                Message = at(3),
                Project = at(4),
                Sub = at(5),
                Code = at(6),
                Op = at(7),
                Path = at(8)
            };
        }

        public static RunSummary Summarize(string path, List<LogEntry> entries)
        {
            var s = new RunSummary { FilePath = path, Started = StartedFromName(path), User = UserFromName(path) };
            if (entries.Count > 0) s.Started = entries[0].Time;

            foreach (var e in entries)
            {
                if (e.Step == "SESSION")
                {
                    if (e.Message.StartsWith("Started", StringComparison.OrdinalIgnoreCase))
                        s.Mode = string.IsNullOrEmpty(e.Path) ? s.Mode : e.Path;
                    else if (e.Message.StartsWith("Finished", StringComparison.OrdinalIgnoreCase))
                    {
                        s.Finished = e.Time;
                        s.Completed = true;
                    }
                }
                else if (e.Step == "LIST" && e.Level == "OK")
                {
                    var m = Regex.Match(e.Message ?? "", @"[Gg]rup[^\d]*(\d+)");
                    if (m.Success) s.Groups = int.Parse(m.Groups[1].Value);
                }
                else if (e.Step == "COPY")
                {
                    s.HasCopy = true;
                    if (e.Level == "OK") s.CopiedOk++;
                    else if (e.Level == "FAIL") s.CopyFail++;
                }
                else if ((e.Step == "EXCEL" || e.Step == "EXCEL_OP") && e.Level == "OK") s.Excel++;
                else if (e.Step == "COPY_ALL" || e.Step == "DONE") s.HasCopy = true;
                else if (e.Step == "CRASH") s.Crashed = true;
                else if (e.Step == "TIMEOUT") s.TimedOut = true;

                if (e.Level == "FAIL") s.Fails++;
                else if (e.Level == "WARN") s.Warns++;
            }
            return s;
        }

        // Run_20251016_093000_kullanici.log
        private static DateTime StartedFromName(string path)
        {
            string n = Path.GetFileNameWithoutExtension(path);
            var m = Regex.Match(n, @"^Run_(\d{8})_(\d{6})");
            DateTime t;
            if (m.Success && DateTime.TryParseExact(m.Groups[1].Value + m.Groups[2].Value, "yyyyMMddHHmmss",
                                                    CultureInfo.InvariantCulture, DateTimeStyles.None, out t))
                return t;
            try { return File.GetCreationTime(path); } catch { return DateTime.MinValue; }
        }

        private static string UserFromName(string path)
        {
            string n = Path.GetFileNameWithoutExtension(path);
            var m = Regex.Match(n, @"^Run_\d{8}_\d{6}_(.+)$");
            return m.Success ? m.Groups[1].Value : "";
        }
    }
}
