using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using ClosedXML.Excel;
using EPDM.Interop.epdm;
using System.Text.RegularExpressions;
using System.Collections.Generic;


namespace ProductionDocumentationSync
{
    public partial class Form1 : Form
    {
        private const string TEMPLATE_XLSX = @"\\192.168.100.66\GebzeUretim\Antet.xlsx";
        private DataTable dtRapor = new DataTable();
        private readonly string connStr = "Server=192.168.10.15;Database=berrmakprod1;User Id=armada;Password=Armada1234*;";
        private readonly Dictionary<string, ListViewItem> _itemsByKey
    =   new Dictionary<string, ListViewItem>(StringComparer.OrdinalIgnoreCase);
        private readonly List<LogRec> _log = new List<LogRec>();
        private const string LOG_DIR = @"\\192.168.100.66\GebzeUretim\_Raporlar";

        //private const string BASE_ROOT = @"C:\Users\fatih.bayram\Desktop\Yeni klasör (2)";
        //private const string LOG_DIR = BASE_ROOT + @"\_Raporlar";
        private readonly Dictionary<string, string> _opCopyStatus =
    new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private XLWorkbook _logWb = null;
        private IXLWorksheet _logWs = null;
        private string _logXlsxFile = null;
        private int _logRow = 0;
        private string _logTxtFile = null;
        private readonly object _logTxtLock = new object();
        private string _reportPath = null;
        private DateTimePicker _dtpFrom, _dtpTo;
        private static IEnumerable<string> PdfSearchOrder(string fullCode, string operation)
        {
            string full = fullCode ?? "";
            string clean = BaseCodeForSearch(full);
            if (IsBoya(operation))
            {
                if (!clean.Equals(full, StringComparison.OrdinalIgnoreCase)) yield return clean;
                yield return full;
            }
            else
            {
                yield return full;
                if (!clean.Equals(full, StringComparison.OrdinalIgnoreCase)) yield return clean;
            }
        }
        
        private static bool IsBoya(string op)
    => !string.IsNullOrWhiteSpace(op) && op.IndexOf("BOYA", StringComparison.OrdinalIgnoreCase) >= 0;
        private class ExcelHeader
        {
            public string Proje;
            public string PozGrupNo;
            public string Tasarimci;
            public string Operasyon;
            public DateTime Tarih;
            public ExcelHeader() { Proje = ""; PozGrupNo = ""; Tasarimci = ""; Operasyon = ""; Tarih = DateTime.Today; }
        }

        private class ExcelItem
        {
            public string HitsoftNo;
            public string UrunKodu;
            public string UrunAdi;
            public string Miktar;   // metin olarak bırakıyoruz
            public string RalKodu;
            public string Aciklama;
            public ExcelItem() { HitsoftNo = ""; UrunKodu = ""; UrunAdi = ""; Miktar = ""; RalKodu = ""; Aciklama = ""; }
        }

        private void WriteExcelFromTemplate(string templatePath, string outPath, ExcelHeader h, List<ExcelItem> items)
        {
            var outDir = Path.GetDirectoryName(outPath);
            if (!string.IsNullOrEmpty(outDir)) Directory.CreateDirectory(outDir);

            using (var wb = new ClosedXML.Excel.XLWorkbook(templatePath))
            {
                var ws = wb.Worksheet(1);

                // Üst bilgiler (şablona göre)
                ws.Cell("B2").Value = h.Proje ?? "";
                ws.Cell("B3").Value = h.PozGrupNo ?? "";
                ws.Cell("B4").Value = h.Tasarimci ?? "";
                ws.Cell("F3").Value = "";
                ws.Cell("F4").Value = "";
                ws.Cell("F5").Value = "";
                ws.Cell("G1").Value = h.Tarih.ToString("dd.MM.yyyy");
                ws.Cell("B1").Value = (h.Operasyon ?? "") + " KONTROL FORMU";

                // Satırlar (A7:G7 şablon)
                int startRow = 7;
                var tpl = ws.Range("A7:G7");
                ws.Range("A7:G10000").Clear(ClosedXML.Excel.XLClearOptions.Contents);

                int r = startRow;
                for (int i = 0; i < items.Count; i++)
                {
                    if (i > 0) tpl.CopyTo(ws.Range("A" + r + ":G" + r));
                    var it = items[i];
                    ws.Cell(r, 1).Value = i + 1;
                    ws.Cell(r, 2).Value = it.HitsoftNo ?? "";
                    ws.Cell(r, 3).Value = it.UrunKodu ?? "";
                    ws.Cell(r, 4).Value = it.UrunAdi ?? "";
                    ws.Cell(r, 5).Value = it.Miktar ?? "";
                    ws.Cell(r, 6).Value = it.RalKodu ?? "";
                    ws.Cell(r, 7).Value = it.Aciklama ?? "";
                    r++;
                }

                wb.SaveAs(outPath);
            }
        }
       

        private static string SafeFile(string s)
        {
            foreach (var c in Path.GetInvalidFileNameChars()) s = s.Replace(c, '_');
            return s;
        }

        // Klasör adı olarak güvenli hale getirir: geçersiz karakterleri _, sondaki nokta/boşlukları kırpar,
        // Windows rezerve adlarını (CON, PRN, NUL, COM1..) saptırır, boş kalırsa "_" döner.
        private static string SafePathSegment(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return "_";
            foreach (var c in Path.GetInvalidFileNameChars()) s = s.Replace(c, '_');
            // Yön ayraçlarını da temizle (Path.Combine yutmasın)
            s = s.Replace('/', '_').Replace('\\', '_');
            // Kontrol karakterlerini at
            var sb = new System.Text.StringBuilder(s.Length);
            foreach (var ch in s) sb.Append(ch < 32 ? '_' : ch);
            s = sb.ToString().Trim().TrimEnd('.', ' ');
            if (s.Length == 0) return "_";
            string upper = s.ToUpperInvariant();
            string[] reserved = { "CON","PRN","AUX","NUL",
                                  "COM1","COM2","COM3","COM4","COM5","COM6","COM7","COM8","COM9",
                                  "LPT1","LPT2","LPT3","LPT4","LPT5","LPT6","LPT7","LPT8","LPT9" };
            foreach (var r in reserved)
                if (upper == r || upper.StartsWith(r + ".")) return "_" + s;
            return s;
        }
        private void Log(string step, string status, string message,
                 string project = null, string sub = null,
                 string baseCode = null, string operation = null, string dest = null)
        {
            LogCsv(status, step, message, project, sub, baseCode, operation, dest);
            _log.Add(new LogRec
            {
                Time = DateTime.Now,
                Step = step,
                Status = status,
                Message = message,
                Project = project,
                SubFolder = sub,
                BaseCode = baseCode,
                Operation = operation,
                DestPath = dest
            });
        }
        private sealed class LogRec
        {
            public DateTime Time { get; set; }
            public string Step { get; set; }          // LIST, COPY, EXCEL, ERROR, DONE, vb.
            public string Project { get; set; }
            public string SubFolder { get; set; }
            public string BaseCode { get; set; }
            public string Operation { get; set; }
            public string Status { get; set; }        // OK / FAIL / INFO
            public string Message { get; set; }       // Detay
            public string DestPath { get; set; }      // Kopyalanan hedef dosya/klasör
        }
        #region log
        private void StartLoggerIfNeeded()
        {
            if (_logWb != null) return;

            Directory.CreateDirectory(LOG_DIR);
            string stamp = $"{DateTime.Now:yyyyMMdd_HHmmss}_{Environment.UserName}";
            _logXlsxFile = Path.Combine(LOG_DIR, $"Run_{stamp}.xlsx");
            _logTxtFile = Path.Combine(LOG_DIR, $"Run_{stamp}.log");

            // Plain-text log header (crash-safe; xlsx Save() çökse bile bu kalır)
            AppendTxtLog("Time\tLevel\tStep\tMessage\tProject\tSub\tBase\tOp\tExtra");

            _logWb = new XLWorkbook();
            _logWs = _logWb.Worksheets.Add("Log");
            _logRow = 1;

            _logWs.Cell(_logRow, 1).Value = "Time";
            _logWs.Cell(_logRow, 2).Value = "Level";
            _logWs.Cell(_logRow, 3).Value = "Step";
            _logWs.Cell(_logRow, 4).Value = "Message";
            _logWs.Cell(_logRow, 5).Value = "Project";
            _logWs.Cell(_logRow, 6).Value = "Sub";
            _logWs.Cell(_logRow, 7).Value = "Base";
            _logWs.Cell(_logRow, 8).Value = "Op";
            _logWs.Cell(_logRow, 9).Value = "Extra";
            _logRow++;

            _logWs.Range(1, 1, 1, 9).Style.Font.Bold = true;
            _logWb.SaveAs(_logXlsxFile);   // path'i set et
        }


        private void LogCsv(string level, string step, string msg,
                    string project = null, string sub = null, string baseCode = null,
                    string op = null, string extra = null)
        {
            try
            {
                StartLoggerIfNeeded();

                // ÖNCE plain-text — xlsx Save çökse bile bu satır diskte olur.
                AppendTxtLog(string.Join("\t", new[] {
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                    level ?? "", step ?? "", msg ?? "",
                    project ?? "", sub ?? "", baseCode ?? "", op ?? "", extra ?? ""
                }.Select(EscapeForTxt)));

                _logWs.Cell(_logRow, 1).SetValue(DateTime.Now);
                _logWs.Cell(_logRow, 2).SetValue(level ?? "");
                _logWs.Cell(_logRow, 3).SetValue(step ?? "");
                _logWs.Cell(_logRow, 4).SetValue(msg ?? "");
                _logWs.Cell(_logRow, 5).SetValue(project ?? "");
                _logWs.Cell(_logRow, 6).SetValue(sub ?? "");
                _logWs.Cell(_logRow, 7).SetValue(baseCode ?? "");
                _logWs.Cell(_logRow, 8).SetValue(op ?? "");
                _logWs.Cell(_logRow, 9).SetValue(extra ?? "");
                _logRow++;

                _logWb.Save(); // anlık yaz (çökse bile kayıtlı kalsın)
                if (_logRow % 50 == 0) _logWs.Columns().AdjustToContents();
            }
            catch { /* sessiz geç */ }
        }

        private static string EscapeForTxt(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Replace("\t", " ").Replace("\r", " ").Replace("\n", " ");
        }

        private void AppendTxtLog(string line)
        {
            if (string.IsNullOrEmpty(_logTxtFile)) return;
            try
            {
                lock (_logTxtLock)
                {
                    // FileShare.ReadWrite + her yazımda flush — crash-safe.
                    using (var fs = new FileStream(_logTxtFile, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                    using (var sw = new StreamWriter(fs, new System.Text.UTF8Encoding(true)))
                    {
                        sw.WriteLine(line);
                        sw.Flush();
                        fs.Flush(true); // OS buffer'ını da diske it
                    }
                }
            }
            catch { /* log için crash etme */ }
        }


        private void StopLogger()
        {
            try
            {
                if (_logWs != null) _logWs.Columns().AdjustToContents();
                _logWb?.Save();
                _logWb?.Dispose();
            }
            catch { }
            finally
            {
                _logWb = null;
                _logWs = null;
            }
        }
        private string EnsureReportFolder()
        {
            // Merkez rapor klasörü
            //string root = @"C:\Users\fatih.bayram\Desktop\Yeni klasör (2)";
            string root = @"\\192.168.100.66\GebzeUretim";

            Directory.CreateDirectory(root);
            return root;
        }

        private void SaveAutoReportIfNeeded()
        {
            if (!isAuto) return;
            try
            {
                string folder = EnsureReportFolder();
                string logsFolder = Path.Combine(folder, "AutoRunLogs");  
                Directory.CreateDirectory(logsFolder);                  
                string ts = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                _reportPath = Path.Combine(logsFolder, $"AutoRun_{ts}.xlsx");

                using (var wb = new XLWorkbook())
                {
                    var ws = wb.Worksheets.Add("ActionLog");
                    string[] headers = { "Time", "Step", "Status", "Message",
                                 "Project", "SubFolder", "BaseCode", "Operation", "DestPath" };
                    for (int i = 0; i < headers.Length; i++)
                        ws.Cell(1, i + 1).Value = headers[i];

                    int r = 2;
                    foreach (var x in _log)
                    {
                        ws.Cell(r, 1).Value = x.Time;
                        ws.Cell(r, 2).Value = x.Step;
                        ws.Cell(r, 3).Value = x.Status;
                        ws.Cell(r, 4).Value = x.Message;
                        ws.Cell(r, 5).Value = x.Project;
                        ws.Cell(r, 6).Value = x.SubFolder;
                        ws.Cell(r, 7).Value = x.BaseCode;
                        ws.Cell(r, 8).Value = x.Operation;
                        ws.Cell(r, 9).Value = x.DestPath;
                        r++;
                    }
                    ws.Columns().AdjustToContents();
                    wb.SaveAs(_reportPath);
                }
            }
            catch (Exception ex)
            {
                // Rapor yazılamadıysa en azından log’a düş
                _log.Add(new LogRec { Time = DateTime.Now, Step = "REPORT", Status = "FAIL", Message = ex.Message });
            }
        }
        #endregion
        public Form1()
        {
            InitializeComponent();
            this.FormClosing += Form1_FormClosing;
        }

        private bool isAuto = false;

        private void Form1_Load(object sender, EventArgs e)
        {
            string[] args = Environment.GetCommandLineArgs();
            isAuto = (args.Length > 1 && args[1].Equals("/auto", StringComparison.OrdinalIgnoreCase));
            if (!isAuto)
            {
                _dtpFrom = new DateTimePicker
                {
                    Format = DateTimePickerFormat.Custom,
                    CustomFormat = "dd.MM.yyyy",
                    Width = 110,
                    Location = new Point(12, 8)
                };
                _dtpTo = new DateTimePicker
                {
                    Format = DateTimePickerFormat.Custom,
                    CustomFormat = "dd.MM.yyyy",
                    Width = 110,
                    Location = new Point(132, 8)
                };
                _dtpFrom.Value = DateTime.Today;
                _dtpTo.Value = DateTime.Today;

                this.Controls.Add(_dtpFrom);
                this.Controls.Add(_dtpTo);
            }
            StartLoggerIfNeeded();                                  // <— her zaman
            LogCsv("INFO", "SESSION", "Started", extra: isAuto ? "AUTO" : "MANUAL"); // <— modu yaz

            if (isAuto)
            {
                // Auto akışı
                btnListele.PerformClick();
                btnKopyala.PerformClick();
                this.Close();
            }

            listView1.View = View.Details;
            listView1.FullRowSelect = true;
            listView1.GridLines = true;
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            LogCsv("INFO", "SESSION", "Finished", extra: _logXlsxFile);
            StopLogger();
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            int totalWidth = listView1.ClientSize.Width;
            int colCount = listView1.Columns.Count;
            int widthPerColumn = totalWidth / colCount;

            foreach (ColumnHeader col in listView1.Columns)
                col.Width = widthPerColumn;
        }
        private static string BaseCodeForSearch(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return "";
            // Sonda -RAL####, -W, -G, -M varsa at
            var m = Regex.Match(code, @"^(.*?)(?:-RAL\d{4}|-W|-G|-M)$", RegexOptions.IgnoreCase);
            return m.Success ? m.Groups[1].Value : code;
        }

        private static string S(object o)
    => (o == null || o == DBNull.Value) ? "" : Convert.ToString(o)?.Trim() ?? "";

        private static string SubFolder(DataRow row)
        {
            string groupNo = S(GetCol(row, "Pos No/Grup Kod", "Sınıf Kodu")); // yeni + eski
            string posNo = S(row["SAP No"]);
            return !string.IsNullOrEmpty(groupNo) ? groupNo : posNo;
        }
        private static string PdfSearchCode(string fullCode, string operation)
        {
            // BOYA için: BaseCodeForSearch uygula (RAL/W/G/M at)
            // Diğer operasyonlarda: tam kodu kullan
            return IsBoya(operation) ? BaseCodeForSearch(fullCode) : fullCode;
        }
        private IEnumerable<IGrouping<string, DataRow>> GroupProducts()
        {
            return dtRapor.AsEnumerable()
                .GroupBy(r =>
                {
                    string project = S(r["Proje"]);
                    string sub = SubFolder(r);
                    string baseCode = BaseCodeForSearch(S(r["Üretilecek Ürün Kodu"]));
                    return $"{project}|{sub}|{baseCode}";
                }, StringComparer.OrdinalIgnoreCase);
        }
        private IEnumerable<IGrouping<string, DataRow>> GroupFolders()
        {
            return dtRapor.AsEnumerable()
                .GroupBy(r =>
                {
                    string project = S(r["Proje"]);
                    string sub = SubFolder(r);
                    return $"{project}|{sub}";
                }, StringComparer.OrdinalIgnoreCase);
        }
        private static string Base(string code) => BaseCodeForSearch(code);
        private IEnumerable<DataRow> DistinctRows()
        {
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var row in dtRapor.AsEnumerable())
            {
                string project = S(row["Proje"]);
                string groupNo = S(GetCol(row, "Pos No/Grup Kod", "Sınıf Kodu"));
                string posNo = S(row["SAP No"]);
                string subFolder = !string.IsNullOrEmpty(groupNo) ? groupNo : posNo;

                string product = S(row["Üretilecek Ürün Kodu"]);
                string baseCode = BaseCodeForSearch(product);

                string kaynak = S(row["Kaynak Kodu"]);   // boş gelebilir

                string key = $"{project}|{subFolder}|{kaynak}|{baseCode}";
                if (seen.Add(key))
                    yield return row; // ilkini al, tekrarları at
            }
        }
        private static object GetCol(DataRow row, params string[] names)
        {
            foreach (var n in names)
                if (row.Table.Columns.Contains(n)) return row[n];
            return null;
        }
        private void btnListele_Click(object sender, EventArgs e)
        {
            try
            {
                dtRapor.Clear();
                listView1.Items.Clear();
                progressBar1.Value = 0;

                // Tarih aralığı (AUTO: bugün; MANUEL: picker'lar)
                DateTime from = isAuto ? DateTime.Today : (_dtpFrom?.Value.Date ?? DateTime.Today);
                DateTime toInc = isAuto ? DateTime.Today : (_dtpTo?.Value.Date ?? DateTime.Today);
                if (toInc < from) { var tmp = from; from = toInc; toInc = tmp; }   // swap
                DateTime toExclusive = toInc.AddDays(1);                            // üst sınır (exclusive)

                LogCsv("INFO", "DATE_RANGE", $">= {from:yyyy-MM-dd} && < {toExclusive:yyyy-MM-dd}");

                using (var conn = new SqlConnection(connStr))
                {
                    conn.Open();

                    string sql = @"
IF OBJECT_ID('tempdb..#GenelUretimRaporu') IS NOT NULL DROP TABLE #GenelUretimRaporu;

SELECT ROW_NUMBER() OVER (ORDER BY T0.DocEntry, T2.ClgCode) AS [Sıra No],
       T0.StartDate AS [Başlangıç Tarihi],
       T1.U_NAME   AS [Kullanıcı],
       T0.Project  AS [Proje],
       T0.DocEntry AS [SAP No],
       CASE T0.Status WHEN 'P' THEN 'Planlandı' WHEN 'R' THEN 'Onaylandı' WHEN 'L' THEN 'Kapalı' ELSE 'Diğer' END AS [Durum],
       T0.PlannedQty AS [Planlanan Miktar],
       T0.ItemCode    AS [Üretilecek Ürün Kodu],
       T0.ProdName    AS [Üretilecek Ürün Adı],
       T3.ItemCode    AS [Üretimde Kullanılan Kalemler],
       CASE
           WHEN T0.U_ClassCode IS NOT NULL AND T0.U_ClassCode <> '' THEN T0.U_ClassCode
           WHEN (T0.U_ClassCode IS NULL OR T0.U_ClassCode = '') AND T0.U_GroupNo IS NOT NULL THEN T0.U_GroupNo
           ELSE NULL
       END AS [Pos No/Grup Kod],
       (SELECT STRING_AGG(ItemCode, '; ')
        FROM WOR1
        JOIN ORSC ON WOR1.ItemCode = ORSC.ResCode AND ORSC.ResGrpCod = 2
        WHERE DocEntry = T0.DocEntry AND ItemType = 290) AS [Kaynak Kodu],
       T2.ClgCode AS [Hitsoft No],
       (SELECT TOP 1 ShipDate
        FROM POR1
        WHERE Project = T0.Project
          AND ItemCode = T3.ItemCode
          AND ShipDate BETWEEN DATEADD(DAY, -7, T0.StartDate) AND DATEADD(DAY, 7, T0.StartDate)
        ORDER BY ShipDate DESC) AS [Termin Tarihi],
       (SELECT TOP 1 OPOR.U_TrackingNum
        FROM POR1
        JOIN OPOR ON POR1.DocEntry = OPOR.DocEntry
        WHERE POR1.Project = T0.Project
          AND POR1.ItemCode = T3.ItemCode
          AND POR1.ShipDate BETWEEN DATEADD(DAY, -7, T0.StartDate) AND DATEADD(DAY, 7, T0.StartDate)
        ORDER BY POR1.ShipDate DESC) AS [SAS Numarası]
INTO #GenelUretimRaporu
FROM OWOR T0
INNER JOIN OUSR T1 ON T1.USERID = T0.UserSign
INNER JOIN OCLG T2 ON T2.DocEntry = T0.DocEntry
INNER JOIN WOR1 T3 ON T3.DocEntry = T0.DocEntry
WHERE T3.ItemType = 4
  AND T0.StartDate >= @From
  AND T0.StartDate <  @To;

SELECT *,
       (SELECT TOP 1 S0.DocDate
        FROM OPDN S0
        JOIN PDN1 S1 ON S0.DocEntry = S1.DocEntry
        JOIN OPOR S2 ON S1.BaseType = S2.ObjType AND S1.BaseEntry = S2.DocEntry
        WHERE S2.U_TrackingNum = T0.[SAS Numarası]
          AND S1.ItemCode = T0.[Üretimde Kullanılan Kalemler]
        ORDER BY S0.DocDate DESC) AS [Mal Kabul Tarihi]
FROM #GenelUretimRaporu T0
ORDER BY 1;";


                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.Add("@From", SqlDbType.DateTime).Value = from;
                        cmd.Parameters.Add("@To", SqlDbType.DateTime).Value = toExclusive; // ertesi gün 00:00
                        using (var rdr = cmd.ExecuteReader())
                        {
                            dtRapor.Load(rdr);
                        }
                    }
                }

                var groups = GroupProducts().ToList();
                progressBar1.Maximum = groups.Count;

                foreach (var g in groups)
                {
                    var first = g.First();

                    string project = S(first["Proje"]);
                    string subFolder = SubFolder(first);
                    string product = S(first["Üretilecek Ürün Kodu"]); // tam kod
                    string status = S(first["Durum"]);
                    string baslangic = Convert.ToDateTime(first["Başlangıç Tarihi"]).ToShortDateString();
                    string miktar = S(first["Planlanan Miktar"]);
                    string urunAdi = S(first["Üretilecek Ürün Adı"]);

                    // sadece görüntü için, birleşik operasyon listesi
                    string opsJoin = string.Join("; ",
                        g.Select(r => S(r["Kaynak Kodu"]))
                         .Where(x => !string.IsNullOrEmpty(x))
                         .Distinct());

                    var item = new ListViewItem(project);
                    item.SubItems.Add(subFolder);
                    item.SubItems.Add(product);
                    item.SubItems.Add(status);
                    item.SubItems.Add("Bekleniyor");
                    item.SubItems.Add(baslangic);
                    item.SubItems.Add(miktar);
                    item.SubItems.Add(urunAdi);
                    item.SubItems.Add(opsJoin);
                    listView1.Items.Add(item);

                    string baseCodeKey = BaseCodeForSearch(product);
                    _itemsByKey[$"{project}|{subFolder}|{baseCodeKey}"] = item;
                }

                Log("LIST", "OK", $"Yüklenen grup sayısı: {groups.Count}");
                LogCsv("OK", "LIST", $"Grup: {groups.Count}");
                if (!isAuto) MessageBox.Show("Veriler yüklendi.");
            }
            catch (Exception ex)
            {
                Log("LIST", "FAIL", ex.Message);
                LogCsv("FAIL", "LIST", ex.Message);
                if (!isAuto) MessageBox.Show("Hata (Listele): " + ex.Message);
            }
        }


        private void btnKopyala_Click(object sender, EventArgs e)
        {
            var folders = GroupFolders().ToList(); // klasör bazlı gruplar
            progressBar1.Maximum = _itemsByKey.Count; // ürün sayısı
            progressBar1.Value = 0;
            int done = 0;

            Log("COPY_ALL", "INFO", $"Toplam {folders.Count} klasör işlenecek");
            LogCsv("INFO", "COPY_ALL", $"Klasör: {folders.Count}");

            _opCopyStatus.Clear();

            foreach (var folder in folders)
            {
                var first = folder.First();
                string project = S(first["Proje"]);
                string subFolder = SubFolder(first);

                // Path için sanitize edilmiş kopyalar (gösterim/log için orijinaller saklanır)
                string projectSafe = SafePathSegment(project);
                string subFolderSafe = SafePathSegment(subFolder);

                // hedef kök
                string targetRoot = Path.Combine(@"\\192.168.100.66\GebzeUretim", projectSafe, subFolderSafe);
                //string targetRoot = Path.Combine(@"C:\Users\fatih.bayram\Desktop\Yeni klasör (2)", projectSafe, subFolderSafe);

                try
                {
                    Directory.CreateDirectory(targetRoot);
                }
                catch (Exception exFolder)
                {
                    LogCsv("FAIL", "FOLDER", "CreateDirectory hata: " + exFolder.Message + " | Path=" + targetRoot,
                           project, subFolder, null, null, targetRoot);
                    Log("FOLDER", "FAIL", exFolder.Message, project, subFolder, null, null, targetRoot);
                    continue; // bu klasörü atla, batch'i öldürme
                }

                Log("FOLDER", "INFO", "Klasör işleniyor", project, subFolder);
                LogCsv("INFO", "FOLDER", "İşleniyor", project, subFolder);

                // --- KOPYALAMA (TAM ÜRÜN KODU ile, BOYA için 2 deneme: clean->full; diğerlerinde full->clean) ---
                var productFullList = folder
                    .Select(r => S(r["Üretilecek Ürün Kodu"]))
                    .Where(x => !string.IsNullOrEmpty(x))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                foreach (var productFull in productFullList)
                {
                    var ops = folder
                        .Where(r => S(r["Üretilecek Ürün Kodu"]).Equals(productFull, StringComparison.OrdinalIgnoreCase))
                        .Select(r => S(r["Kaynak Kodu"]))
                        .Where(x => !string.IsNullOrEmpty(x))
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToList();

                    Log("COPY_GROUP", "INFO", "Base ürün kopyalama başlıyor", project, subFolder, productFull);
                    LogCsv("INFO", "COPY_GROUP", "Başlıyor", project, subFolder, productFull);

                    string finalStatus = "Kopyalandı";

                    foreach (var op in ops)
                    {
                        string opSafe = SafePathSegment(op);
                        string opFolder = Path.Combine(targetRoot, opSafe);
                        try
                        {
                            Directory.CreateDirectory(opFolder);
                        }
                        catch (Exception exOp)
                        {
                            LogCsv("FAIL", "FOLDER_OP", "CreateDirectory hata: " + exOp.Message + " | Path=" + opFolder,
                                   project, subFolder, productFull, op, opFolder);
                            continue; // bu op'u atla
                        }

                        string st = "PDF bulunamadı";
                        string usedCode = null;

                        // iki aşamalı deneme
                        foreach (var code in PdfSearchOrder(productFull, op))
                        {
                            var tryRes = FindAndCopyPdfFromPdm(code, opFolder, project, subFolder);
                            usedCode = code;
                            if (tryRes.StartsWith("Kopyalandı", StringComparison.OrdinalIgnoreCase))
                            {
                                st = tryRes; break;
                            }
                            st = tryRes; // son mesaj
                        }

                        if (!st.StartsWith("Kopyalandı", StringComparison.OrdinalIgnoreCase))
                            finalStatus = st;

                        _opCopyStatus[$"{project}|{subFolder}|{productFull}|{op}"] = st;

                        LogCsv(st.StartsWith("Kopyalandı", StringComparison.OrdinalIgnoreCase) ? "OK" : "FAIL",
                               "COPY", st, project, subFolder, productFull, op,
                               Path.Combine(opFolder, (usedCode ?? productFull) + ".pdf"));
                    }

                    // ListView "Dosya Durumu" güncelle (anahtar base’e göre)
                    string baseKey = BaseCodeForSearch(productFull);
                    string key = $"{project}|{subFolder}|{baseKey}";
                    if (_itemsByKey.TryGetValue(key, out var li))
                    {
                        li.SubItems[4].Text = finalStatus;
                        li.ForeColor = finalStatus.StartsWith("Kopyalandı", StringComparison.OrdinalIgnoreCase) ? System.Drawing.Color.Black : System.Drawing.Color.Red;
                    }

                    progressBar1.Value = Math.Min(progressBar1.Maximum, ++done);
                }

                // ==========================
                // EXCEL (üst klasör): satır bazlı, net mükerrer tanımı, RAL taşıma
                // ==========================

                // BOYA’daki RAL -> baseCode eşlemesi (ilk görüleni al)
                var boyaRalByBase = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                var galvanizByBase = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (DataRow r in folder)
                {
                    string op = S(r["Kaynak Kodu"]);
                    string code = S(r["Üretilecek Ürün Kodu"]);
                    if (string.IsNullOrEmpty(code)) continue;

                    string baseCode = BaseCodeForSearch(code);

                    // 1) -G varyantı veya operasyon adı "GALVAN" içeriyorsa GALVANIZ var say
                    if (Regex.IsMatch(code, @"-G$", RegexOptions.IgnoreCase) ||
                        (!string.IsNullOrEmpty(op) && op.IndexOf("GALVAN", StringComparison.OrdinalIgnoreCase) >= 0))
                    {
                        galvanizByBase.Add(baseCode);
                    }

                    // 2) BOYA satırından RAL yakala (örn. ...-RAL7035)
                    if (IsBoya(op))
                    {
                        var m = Regex.Match(code, @"-RAL(\d{4})$", RegexOptions.IgnoreCase);
                        if (m.Success && !boyaRalByBase.ContainsKey(baseCode))
                            boyaRalByBase[baseCode] = "RAL" + m.Groups[1].Value;
                    }
                }
                // satırları mükerrer anahtarla teke indir (hiçbir toplama/birleştirme yok)
                var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                var excelRows = new List<(string Proje, string Klasor, string SapNo, string HitsoftNo,
                                          string UrunKodu, string Durum, string DosyaDurumu,
                                          DateTime Baslangic, string Miktar, string UrunAdi,
                                          string Operasyon, string RalKoduForKaynak)>();

                foreach (DataRow r in folder)
                {
                    string proje = S(r["Proje"]);
                    string klasor = SubFolder(r); // Sınıf Kodu varsa o, yoksa SAP
                    string sapNo = S(r["SAP No"]);
                    string hitsoftNo = S(r["Hitsoft No"]);
                    string urunKodu = S(r["Üretilecek Ürün Kodu"]); // TAM KOD
                    string durum = S(r["Durum"]);
                    string miktar = S(r["Planlanan Miktar"]);      // olduğu gibi; toplama yok
                    string urunAdi = S(r["Üretilecek Ürün Adı"]);
                    string op = S(r["Kaynak Kodu"]);
                    DateTime baslangic = Convert.ToDateTime(r["Başlangıç Tarihi"]);

                    // MÜKERRER anahtar: 8 alan bire bir aynıysa tek satır
                    string dupKey = string.Join("|", proje, klasor, sapNo, hitsoftNo, urunKodu, miktar, urunAdi, op);
                    if (!seen.Add(dupKey)) continue;

                    // dosya durumu (kopyalama sonucu)
                    string st = _opCopyStatus.TryGetValue($"{proje}|{klasor}|{urunKodu}|{op}", out var vv) ? vv : "İşlenmedi";

                    // KAYNAK satırına BOYA RAL taşı
                    string ralForKaynak = "";
                    if (!string.IsNullOrEmpty(op) && op.IndexOf("KAYNAK", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        string baseCode = BaseCodeForSearch(urunKodu);

                        if (galvanizByBase.Contains(baseCode))
                            ralForKaynak = "GALVANIZ";                       // öncelik GALVANIZ
                        else if (boyaRalByBase.TryGetValue(baseCode, out var ral))
                            ralForKaynak = ral;                              // aksi halde RALxxxx
                    }

                    excelRows.Add((proje, klasor, sapNo, hitsoftNo, urunKodu, durum, st,
                                   baslangic, miktar, urunAdi, op, ralForKaynak));
                }

                // Üst klasör Excel
                // === ÜST KLASÖR EXCEL: ŞABLONDAN ===
                try
                {
                    var tasarimci = S(first["Kullanıcı"]); // üst bilgi dinamik
                    var headerTop = new ExcelHeader
                    {
                        Proje = project,
                        PozGrupNo = subFolder,      // Sınıf Kodu varsa o, yoksa SAP
                        Tasarimci = tasarimci,
                        Operasyon = "GENEL",
                        Tarih = DateTime.Today
                    };

                    var itemsTop = new List<ExcelItem>();
                    foreach (var x in excelRows)
                    {
                        itemsTop.Add(new ExcelItem
                        {
                            HitsoftNo = x.HitsoftNo,
                            UrunKodu = x.UrunKodu,
                            UrunAdi = x.UrunAdi,
                            Miktar = x.Miktar,                 // metin bırak
                            RalKodu = x.RalKoduForKaynak,       // BOYA->KAYNAK taşınan RAL
                            Aciklama = ""
                        });
                    }

                    WriteExcelFromTemplate(
                        TEMPLATE_XLSX,
                        Path.Combine(targetRoot, "Urunler.xlsx"),
                        headerTop,
                        itemsTop
                    );

                    LogCsv("OK", "EXCEL", "Urunler.xlsx", project, subFolder, null, null,
                           Path.Combine(targetRoot, "Urunler.xlsx"));

                    // === OPERASYON EXCEL'LERİ: ŞABLONDAN ===
                    var byOp = excelRows
                        .Where(x => !string.IsNullOrWhiteSpace(x.Operasyon) && x.Operasyon != "-")
                        .GroupBy(x => x.Operasyon, StringComparer.OrdinalIgnoreCase);

                    foreach (var g in byOp)
                    {
                        string op = g.Key;
                        string opSafe = SafePathSegment(op);
                        string opDir = Path.Combine(targetRoot, opSafe);
                        try
                        {
                            Directory.CreateDirectory(opDir);
                        }
                        catch (Exception exOpDir)
                        {
                            LogCsv("FAIL", "FOLDER_OP_EXCEL", "CreateDirectory hata: " + exOpDir.Message + " | Path=" + opDir,
                                   project, subFolder, null, op, opDir);
                            continue;
                        }

                        var headerOp = new ExcelHeader
                        {
                            Proje = project,
                            PozGrupNo = subFolder,
                            Tasarimci = tasarimci,
                            Operasyon = op,
                            Tarih = DateTime.Today
                        };

                        var itemsOp = new List<ExcelItem>();
                        foreach (var row in g.OrderBy(x => x.Proje).ThenBy(x => x.Klasor).ThenBy(x => x.UrunKodu))
                        {
                            itemsOp.Add(new ExcelItem
                            {
                                HitsoftNo = row.HitsoftNo,
                                UrunKodu = row.UrunKodu,
                                UrunAdi = row.UrunAdi,
                                Miktar = row.Miktar,
                                RalKodu = row.RalKoduForKaynak,
                                Aciklama = ""
                            });
                        }

                        WriteExcelFromTemplate(
                            TEMPLATE_XLSX,
                            Path.Combine(opDir, "Urunler_" + opSafe + ".xlsx"),
                            headerOp,
                            itemsOp
                        );

                        LogCsv("OK", "EXCEL_OP", "Urunler_" + opSafe + ".xlsx",
                               project, subFolder, null, op,
                               Path.Combine(opDir, "Urunler_" + opSafe + ".xlsx"));
                    }
                }
                catch (Exception ex)
                {
                    LogCsv("FAIL", "EXCEL", "Şablondan yazım hatası: " + ex.Message, project, subFolder);
                }

            }

            Log("DONE", "OK", "Kopyalama tamamlandı");
            LogCsv("OK", "DONE", "Kopyalama tamamlandı");
            SaveAutoReportIfNeeded();
        }



        private string FindAndCopyPdfFromPdm(string productCodeRaw, string targetFolder, string projectHint = null, string subFolderHint = null)
        {
            string fileNameToFind = productCodeRaw + ".pdf";

            try
            {
                IEdmVault5 vault = new EdmVault5();
                if (!vault.IsLoggedIn)
                    vault.LoginAuto("BERRMAK", 0);

                // Arama
                IEdmSearch5 search = (IEdmSearch5)vault.CreateSearch();
                search.FileName = fileNameToFind;
                search.FindFiles = true;
                search.FindFolders = false;

                // Adayları topla + puanla
                var candidates = new List<(string path, int score)>();
                
                for (IEdmSearchResult5 res = search.GetFirstResult(); res != null; res = search.GetNextResult())
                {
                    string p = res.Path;
                    int s = 0;
                    if (!string.IsNullOrEmpty(projectHint) && p.IndexOf(projectHint, StringComparison.OrdinalIgnoreCase) >= 0) s += 10;
                    if (!string.IsNullOrEmpty(subFolderHint) && p.IndexOf(subFolderHint, StringComparison.OrdinalIgnoreCase) >= 0) s += 3;
                    s += Math.Min(5, p.Count(ch => ch == '\\'));
                    candidates.Add((p, s));
                    LogCsv("INFO", "PDM", $"CandidateCount={candidates.Count}", null, null, productCodeRaw);

                }
                if (candidates.Count == 0)
                {
                    LogCsv("FAIL", "PDM", "No candidates", null, null, productCodeRaw);
                    return "PDF bulunamadı";
                }

                // Hedef klasörü hazırla (UNC olabilir)
                Directory.CreateDirectory(targetFolder);
                string outDir = targetFolder.EndsWith("\\") ? targetFolder : (targetFolder + "\\");

                // En iyi adaydan başlayıp tek tek dene
                foreach (var cand in candidates.OrderByDescending(c => c.score).ThenByDescending(c => c.path.Length))
                {
                    IEdmFolder5 parent;
                    var file = (IEdmFile5)vault.GetFileFromPath(cand.path, out parent);
                    if (file == null) continue;

                   
                    
                    string newName = productCodeRaw + ".pdf";

                    try
                    {
                        file.GetFileCopy(0, 0, outDir,0, newName);
                        // Başarılı geldiyse hedefte dosya olmalı
                        string destPath = Path.Combine(outDir, newName);
                        if (File.Exists(destPath))
                            return "Kopyalandı";

                    }
                    catch (System.Runtime.InteropServices.COMException comEx)
                    {
                        LogCsv("WARN", "PDM_COPY", "COM: " + comEx.Message, null, null, productCodeRaw, null, cand.path);
                        continue;
                    }
                    catch (Exception ex)
                    {
                        LogCsv("WARN", "PDM_COPY", "EX: " + ex.Message, null, null, productCodeRaw, null, cand.path);
                        continue;
                    }
                }
                LogCsv("FAIL", "PDM_COPY", "All candidates failed", null, null, productCodeRaw);
                return "İndirilemedi (GetFileCopy başarısız)";
            }
            catch (Exception ex)
            {
                return "Hata: " + ex.Message;
            }
        }



    }
}
