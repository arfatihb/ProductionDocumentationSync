using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;

namespace ProductionDocumentationSync
{
    /// <summary>Windows Görev Zamanlayıcı'daki görevin özeti.</summary>
    internal sealed class ScheduledTaskInfo
    {
        public bool Exists;
        public bool Enabled;
        public int State;                 // 1 Devre dışı, 2 Kuyrukta, 3 Hazır, 4 Çalışıyor
        public DateTime? NextRun;
        public DateTime? LastRun;
        public int? LastResult;
        public TimeSpan Time = new TimeSpan(7, 0, 0);
        public int DaysMask = ScheduledTaskManager.AllDays;
        public string ExePath = "";
        public string Arguments = "";
        public string WorkingDirectory = "";
        public string UserId = "";
        public string Error;              // null = sorgu başarılı

        public string StateText
        {
            get
            {
                switch (State)
                {
                    case 1: return "Devre dışı";
                    case 2: return "Kuyrukta";
                    case 3: return "Hazır";
                    case 4: return "Çalışıyor";
                    default: return "Bilinmiyor";
                }
            }
        }

        public string ScheduleText
        {
            get { return ScheduledTaskManager.DescribeSchedule(Time, DaysMask); }
        }
    }

    /// <summary>
    /// Windows Görev Zamanlayıcı (Schedule.Service COM API, geç bağlama) ile günlük /auto görevini kurar, sorgular, siler.
    /// Görev "yalnızca kullanıcı oturum açtığında" (interactive token) çalışacak şekilde kurulur: PDM istemcisi
    /// etkileşimsiz oturumda çalışmaz. Parola gerekmez, yönetici yetkisi gerekmez (kendi kullanıcısı için).
    /// </summary>
    internal static class ScheduledTaskManager
    {
        public static string TaskName = "ProductionDocumentationSync";
        public const int AllDays = 0x7F;

        // Görev Zamanlayıcı bit maskesi: Paz=1, Pzt=2, Sal=4, Çar=8, Per=16, Cum=32, Cmt=64 (dizi sırası Pzt..Paz)
        public static readonly int[] DayBits = { 2, 4, 8, 16, 32, 64, 1 };
        public static readonly string[] DayNames = { "Pzt", "Sal", "Çar", "Per", "Cum", "Cmt", "Paz" };

        private const int TASK_TRIGGER_DAILY = 2;
        private const int TASK_TRIGGER_WEEKLY = 3;
        private const int TASK_ACTION_EXEC = 0;
        private const int TASK_CREATE_OR_UPDATE = 6;
        private const int TASK_LOGON_INTERACTIVE_TOKEN = 3;
        private const int TASK_RUNLEVEL_LUA = 0;
        private const int TASK_INSTANCES_IGNORE_NEW = 2;

        private static dynamic Connect()
        {
            var t = Type.GetTypeFromProgID("Schedule.Service");
            if (t == null) throw new InvalidOperationException("Görev Zamanlayıcı servisi (Schedule.Service) bulunamadı.");
            dynamic svc = Activator.CreateInstance(t);
            svc.Connect();
            return svc;
        }

        public static ScheduledTaskInfo Query()
        {
            var info = new ScheduledTaskInfo();
            try
            {
                dynamic svc = Connect();
                dynamic root = svc.GetFolder("\\");
                dynamic task;
                try { task = root.GetTask(TaskName); }
                catch (FileNotFoundException) { return info; }                  // 0x80070002 bu tipe eşlenir
                catch (COMException ex)
                {
                    if (unchecked((uint)ex.ErrorCode) == 0x80070002u) return info;
                    throw;
                }

                info.Exists = true;
                info.Enabled = (bool)task.Enabled;
                info.State = (int)task.State;
                info.NextRun = AsDate(task.NextRunTime);
                info.LastRun = AsDate(task.LastRunTime);
                try { info.LastResult = unchecked((int)Convert.ToInt64(task.LastTaskResult)); } catch { }

                dynamic def = task.Definition;
                try { info.UserId = (string)def.Principal.UserId ?? ""; } catch { }

                dynamic trigs = def.Triggers;
                int n = (int)trigs.Count;
                for (int i = 1; i <= n; i++)
                {
                    dynamic tr = trigs.Item(i);
                    int type = (int)tr.Type;
                    if (type != TASK_TRIGGER_DAILY && type != TASK_TRIGGER_WEEKLY) continue;
                    string sb = (string)tr.StartBoundary;
                    DateTime dt;
                    if (DateTime.TryParse(sb, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out dt))
                        info.Time = dt.TimeOfDay;
                    info.DaysMask = type == TASK_TRIGGER_WEEKLY ? ((int)tr.DaysOfWeek & AllDays) : AllDays;
                    break;
                }

                dynamic acts = def.Actions;
                if ((int)acts.Count >= 1)
                {
                    dynamic a = acts.Item(1);
                    if ((int)a.Type == TASK_ACTION_EXEC)
                    {
                        info.ExePath = (string)a.Path ?? "";
                        info.Arguments = (string)a.Arguments ?? "";
                        info.WorkingDirectory = (string)a.WorkingDirectory ?? "";
                    }
                }
            }
            catch (Exception ex)
            {
                info.Error = ex.Message;
            }
            return info;
        }

        /// <summary>Görevi kurar ya da günceller. daysMask: DayBits bileşimi (AllDays = her gün).</summary>
        public static void Register(TimeSpan time, int daysMask, string exePath, string arguments)
        {
            daysMask &= AllDays;
            if (daysMask == 0) throw new ArgumentException("En az bir gün seçilmeli.");
            if (string.IsNullOrEmpty(exePath) || !File.Exists(exePath)) throw new FileNotFoundException("Program bulunamadı: " + exePath);

            string user = Environment.UserDomainName + "\\" + Environment.UserName;

            dynamic svc = Connect();
            dynamic root = svc.GetFolder("\\");
            dynamic def = svc.NewTask(0);

            def.RegistrationInfo.Description = "SAP iş emirlerine göre PDM'den PDF ve Excel üretir (ProductionDocumentationSync /auto).";
            def.RegistrationInfo.Author = user;

            def.Principal.UserId = user;
            def.Principal.LogonType = TASK_LOGON_INTERACTIVE_TOKEN;   // "yalnızca kullanıcı oturum açtığında"
            def.Principal.RunLevel = TASK_RUNLEVEL_LUA;

            dynamic s = def.Settings;
            s.Enabled = true;
            s.AllowDemandStart = true;
            s.StartWhenAvailable = true;                 // saat kaçırıldıysa (uyku vb.) ilk fırsatta çalıştır
            s.DisallowStartIfOnBatteries = false;
            s.StopIfGoingOnBatteries = false;
            s.RunOnlyIfNetworkAvailable = false;
            s.MultipleInstances = TASK_INSTANCES_IGNORE_NEW;
            s.ExecutionTimeLimit = "PT3H";
            s.Hidden = false;

            DateTime start = DateTime.Today.Add(time);
            if (start <= DateTime.Now) start = start.AddDays(1);
            string startBoundary = start.ToString("yyyy-MM-dd'T'HH:mm:ss", CultureInfo.InvariantCulture);

            if (daysMask == AllDays)
            {
                dynamic tr = def.Triggers.Create(TASK_TRIGGER_DAILY);
                tr.StartBoundary = startBoundary;
                tr.DaysInterval = (short)1;
                tr.Enabled = true;
            }
            else
            {
                dynamic tr = def.Triggers.Create(TASK_TRIGGER_WEEKLY);
                tr.StartBoundary = startBoundary;
                tr.WeeksInterval = (short)1;
                tr.DaysOfWeek = (short)daysMask;
                tr.Enabled = true;
            }

            dynamic act = def.Actions.Create(TASK_ACTION_EXEC);
            act.Path = exePath;
            act.Arguments = arguments ?? "";
            act.WorkingDirectory = Path.GetDirectoryName(exePath);

            root.RegisterTaskDefinition(TaskName, def, TASK_CREATE_OR_UPDATE, null, null, TASK_LOGON_INTERACTIVE_TOKEN, null);
        }

        public static void Remove()
        {
            dynamic svc = Connect();
            dynamic root = svc.GetFolder("\\");
            root.DeleteTask(TaskName, 0);
        }

        /// <summary>Görevi hemen başlatır (Görev Zamanlayıcı üzerinden; sonuç görevde görünür).</summary>
        public static void RunNow()
        {
            dynamic svc = Connect();
            dynamic root = svc.GetFolder("\\");
            dynamic task = root.GetTask(TaskName);
            task.Run(null);
        }

        public static string DescribeSchedule(TimeSpan time, int daysMask)
        {
            string hhmm = new DateTime(2000, 1, 1).Add(time).ToString("HH:mm");
            daysMask &= AllDays;
            if (daysMask == AllDays) return "Her gün " + hhmm;
            var names = new List<string>();
            for (int i = 0; i < DayBits.Length; i++)
                if ((daysMask & DayBits[i]) != 0) names.Add(DayNames[i]);
            return (names.Count == 0 ? "(gün seçilmedi)" : string.Join(", ", names)) + " " + hhmm;
        }

        /// <summary>Görev Zamanlayıcı "Son Çalıştırma Sonucu" değerini açıklar (uygulamanın çıkış kodları dahil).</summary>
        public static string DescribeResult(int code)
        {
            switch (code)
            {
                case 0: return "Başarılı";
                case 1: return "Çökme (günlüğe bakın)";
                case 2: return "Tamamlandı, kopyalanamayan PDF var";
                case 3: return "Zaman aşımı";
                case 4: return "Zaten çalışıyordu, atlandı";
                case 5: return "Yapılandırma hatası";
                case 267008: return "Hazır";                                   // 0x41300
                case 267009: return "Şu an çalışıyor";                         // 0x41301
                case 267011: return "Henüz hiç çalışmadı";                     // 0x41303
                case 267014: return "Kullanıcı tarafından durduruldu";         // 0x41306
                case unchecked((int)0x800710E0): return "Kullanıcı oturum açık değildi, görev çalıştırılamadı";
                case unchecked((int)0x8007010B): return "Başlangıç klasörü bulunamadı";
                case unchecked((int)0x80070002): return "Program dosyası bulunamadı";
                default: return "0x" + code.ToString("X8");
            }
        }

        private static DateTime? AsDate(object o)
        {
            try
            {
                if (o == null) return null;
                DateTime d = Convert.ToDateTime(o);
                return d.Year <= 2000 ? (DateTime?)null : d;   // hiç çalışmadıysa 30.12.1899 ya da 30.11.1999 döner
            }
            catch { return null; }
        }
    }
}
