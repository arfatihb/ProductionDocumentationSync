using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;

namespace ProductionDocumentationSync
{
    /// <summary>
    /// Tüm yollar ve bağlantı bilgileri ProductionDocumentationSync.exe.config (App.config) dosyasından okunur.
    /// Kodda sabit yol yoktur; sunucu değişince yalnızca config düzenlenir.
    /// </summary>
    internal static class AppConfig
    {
        public const string DefaultTargetRoot = @"D:\GebzeUretim";

        /// <summary>Kopyalanan PDF/Excel dosyalarının kök klasörü (Proje\Klasör\Operasyon altında).</summary>
        public static string TargetRoot { get; private set; }
        /// <summary>Excel şablonu (Antet.xlsx). Boşsa TargetRoot\Antet.xlsx.</summary>
        public static string TemplateXlsx { get; private set; }
        /// <summary>Çalışma günlükleri (Run_*.log / Run_*.xlsx). Boşsa TargetRoot\_Raporlar.</summary>
        public static string LogDir { get; private set; }
        /// <summary>SAP veritabanı bağlantısı (connectionStrings/Sap).</summary>
        public static string ConnectionString { get; private set; }
        /// <summary>SOLIDWORKS PDM kasa adı.</summary>
        public static string PdmVault { get; private set; }
        /// <summary>/auto modunda bugünden kaç gün geriye bakılacağı (0 = sadece bugün).</summary>
        public static int AutoLookbackDays { get; private set; }
        /// <summary>/auto modunda azami çalışma süresi (dakika). 0 = sınırsız.</summary>
        public static int AutoTimeoutMinutes { get; private set; }
        /// <summary>/auto modunda iş bitince pencerenin özetle açık kalacağı süre (saniye). 0 = hemen kapan.</summary>
        public static int AutoCloseDelaySeconds { get; private set; }

        static AppConfig()
        {
            TargetRoot = Get("TargetRoot", DefaultTargetRoot);
            TemplateXlsx = Get("TemplateXlsx", Path.Combine(TargetRoot, "Antet.xlsx"));
            LogDir = Get("LogDir", Path.Combine(TargetRoot, "_Raporlar"));
            PdmVault = Get("PdmVault", "BERRMAK");
            AutoLookbackDays = GetInt("AutoLookbackDays", 0);
            AutoTimeoutMinutes = GetInt("AutoTimeoutMinutes", 120);
            AutoCloseDelaySeconds = GetInt("AutoCloseDelaySeconds", 10);

            string cs = null;
            try
            {
                var entry = ConfigurationManager.ConnectionStrings["Sap"];
                if (entry != null) cs = entry.ConnectionString;
            }
            catch { /* config bozuksa Validate yakalar */ }
            ConnectionString = (cs ?? "").Trim();
        }

        private static string Get(string key, string defaultValue)
        {
            try
            {
                string v = ConfigurationManager.AppSettings[key];
                return string.IsNullOrWhiteSpace(v) ? defaultValue : v.Trim();
            }
            catch { return defaultValue; }
        }

        private static int GetInt(string key, int defaultValue)
        {
            int n;
            return int.TryParse(Get(key, null), out n) ? n : defaultValue;
        }

        public sealed class Problem
        {
            public bool Fatal;
            public string Message;
        }

        /// <summary>
        /// Başlangıç kontrolü. Fatal olanlar uygulamanın çalışmasını engeller; diğerleri uyarıdır.
        /// </summary>
        public static List<Problem> Validate()
        {
            var list = new List<Problem>();

            if (string.IsNullOrWhiteSpace(TargetRoot) || !Path.IsPathRooted(TargetRoot))
                list.Add(new Problem { Fatal = true, Message = "TargetRoot geçersiz: '" + TargetRoot + "' (tam yol olmalı, örn. D:\\GebzeUretim)" });
            else
            {
                try { Directory.CreateDirectory(TargetRoot); }
                catch (Exception ex)
                {
                    list.Add(new Problem { Fatal = true, Message = "Hedef klasör oluşturulamadı/erişilemiyor: " + TargetRoot + " (" + ex.Message + ")" });
                }
            }

            if (!File.Exists(TemplateXlsx))
                list.Add(new Problem { Fatal = false, Message = "Excel şablonu bulunamadı: " + TemplateXlsx + " — PDF'ler kopyalanır ama Urunler.xlsx üretilemez." });

            if (string.IsNullOrWhiteSpace(ConnectionString))
                list.Add(new Problem { Fatal = true, Message = "connectionStrings/Sap tanımlı değil (SAP veritabanı bağlantısı)." });

            if (string.IsNullOrWhiteSpace(PdmVault))
                list.Add(new Problem { Fatal = true, Message = "PdmVault (PDM kasa adı) boş." });

            return list;
        }

        /// <summary>AutoLookbackDays değerini exe.config'e yazar (uygulama içinden ayarlanabilsin diye).</summary>
        public static void SaveAutoLookbackDays(int days)
        {
            days = Math.Max(0, Math.Min(365, days));
            var cfg = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var s = cfg.AppSettings.Settings;
            if (s["AutoLookbackDays"] == null) s.Add("AutoLookbackDays", days.ToString());
            else s["AutoLookbackDays"].Value = days.ToString();
            cfg.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
            AutoLookbackDays = days;
        }

        public static string Describe()
        {
            return "TargetRoot=" + TargetRoot +
                   " | Template=" + TemplateXlsx +
                   " | LogDir=" + LogDir +
                   " | Vault=" + PdmVault +
                   " | AutoLookbackDays=" + AutoLookbackDays +
                   " | AutoTimeoutMinutes=" + AutoTimeoutMinutes +
                   " | AutoCloseDelaySeconds=" + AutoCloseDelaySeconds;
        }
    }
}
