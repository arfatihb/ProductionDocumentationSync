using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using ClosedXML.Excel;
using EPDM.Interop.epdm;

namespace ProductionDocumentationSync
{
    public partial class Form1 : Form
    {
        // ------------------------------------------------------------------
        // Durum
        // ------------------------------------------------------------------
        private readonly bool _isAuto;
        private RunLogger _logger;
        private DataTable _dtRapor = new DataTable();
        private readonly Dictionary<string, ListViewItem> _itemsByKey =
            new Dictionary<string, ListViewItem>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, string> _opCopyStatus =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        private volatile bool _busy;
        private bool _configFatal;
        private bool _autoDone;
        private System.Windows.Forms.Timer _autoWatchdog;
        private IEdmVault5 _vault;                  // yalnızca işçi iş parçacığında kullanılır

        private int _runOk, _runFail, _runExcel;    // son kopyalama turunun sayaçları
        private int _lastGroups = -1;               // son listelemedeki grup sayısı

        // Günlük sekmesi
        private List<LogEntry> _shownLines = new List<LogEntry>();
        private long _lastShownSeq;
        private const int MaxShownLines = 20000;

        public Form1() : this(false) { }

        public Form1(bool isAuto)
        {
            _isAuto = isAuto;
            InitializeComponent();
            InitHistoryColumns();
        }

        // ------------------------------------------------------------------
        // Form olayları
        // ------------------------------------------------------------------
        private void Form1_Load(object sender, EventArgs e)
        {
            _logger = new RunLogger(AppConfig.LogDir, _isAuto ? "AUTO" : "MANUAL");
            RunLogger.Current = _logger;
            _logger.EntryAdded += OnLogEntryFromAnyThread;

            dtpFrom.Value = _isAuto ? DateTime.Today.AddDays(-Math.Max(0, AppConfig.AutoLookbackDays)) : DateTime.Today;
            dtpTo.Value = DateTime.Today;
            lblMode.Text = _isAuto ? "Mod: OTOMATİK" : "Mod: MANUEL";
            lblHedef.Text = "Hedef: " + AppConfig.TargetRoot;
            Text = "Üretim Doküman Senkronizasyonu" + (_isAuto ? " — OTOMATİK" : "");
            if (_isAuto) tabControl1.SelectedTab = tabGunluk;   // otomatikte canlı günlük görünsün

            _logger.Log("INFO", "SESSION", "Started", path: _logger.Mode);
            _logger.Log("INFO", "CONFIG", AppConfig.Describe()
                + " | Makine=" + Environment.MachineName
                + " | Kullanıcı=" + Environment.UserName
                + " | Sürüm=" + Application.ProductVersion);
            if (!string.IsNullOrEmpty(_logger.FallbackReason))
                _logger.Log("WARN", "CONFIG", _logger.FallbackReason);

            var problems = AppConfig.Validate();
            foreach (var p in problems)
            {
                _logger.Log(p.Fatal ? "FAIL" : "WARN", "CONFIG", p.Message);
                _configFatal |= p.Fatal;
            }

            RefreshRuns();
            SetStatus("Hazır");

            if (_configFatal)
            {
                btnListele.Enabled = btnKopyala.Enabled = btnBasla.Enabled = false;
                SetStatus("Yapılandırma hatası — Çalışma Günlüğü sekmesine bakın");
                if (_isAuto) { FinishAuto(5, "CONFIG hatası"); return; }

                string msg = "Yapılandırma hatası:\n- "
                    + string.Join("\n- ", problems.Where(p => p.Fatal).Select(p => p.Message))
                    + "\n\nProductionDocumentationSync.exe.config dosyasını düzeltip uygulamayı yeniden başlatın.";
                MessageBox.Show(this, msg, "Yapılandırma", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (problems.Count > 0)
            {
                SetStatus("Uyarı: " + problems[0].Message);
            }
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            if (!_isAuto || _autoDone) return;

            int minutes = AppConfig.AutoTimeoutMinutes;
            if (minutes > 0)
            {
                _autoWatchdog = new System.Windows.Forms.Timer();
                _autoWatchdog.Interval = (int)Math.Min(int.MaxValue, (long)minutes * 60L * 1000L);
                _autoWatchdog.Tick += AutoWatchdog_Tick;
                _autoWatchdog.Start();
            }

            // Load/Shown içinde doğrudan değil; mesaj döngüsü boşalınca başlat.
            BeginInvoke(new Action(() => StartRun(true, true)));
        }

        private void AutoWatchdog_Tick(object sender, EventArgs e)
        {
            _autoWatchdog.Stop();
            _logger.Log("FAIL", "TIMEOUT", "Azami süre aşıldı (" + AppConfig.AutoTimeoutMinutes + " dk); uygulama kapatılıyor");
            _logger.Finish("TIMEOUT | ExitCode=3");
            Environment.Exit(3);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_busy && !_isAuto)
            {
                var r = MessageBox.Show(this, "Bir işlem sürüyor. Yine de çıkılsın mı?", "Çıkış",
                                        MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (r != DialogResult.Yes) { e.Cancel = true; return; }
                _logger.Log("WARN", "SESSION", "İşlem sürerken kullanıcı kapattı");
            }
            if (_autoWatchdog != null) _autoWatchdog.Stop();
            if (_logger != null)
            {
                _logger.EntryAdded -= OnLogEntryFromAnyThread;
                _logger.Finish("Kopyalandı: " + _logger.OkCopies + ", Hatalı: " + _logger.FailCopies + ", Excel: " + _logger.ExcelCount);
            }
        }

        private void listView1_Resize(object sender, EventArgs e)
        {
            double[] w = { 1.0, 1.0, 1.3, 0.8, 1.2, 0.8, 0.6, 1.8, 1.3 };
            int total = listView1.ClientSize.Width - 4;
            if (total <= 0 || listView1.Columns.Count != w.Length) return;
            double sum = w.Sum();
            for (int i = 0; i < w.Length; i++)
                listView1.Columns[i].Width = (int)(total * w[i] / sum);
        }

        // ------------------------------------------------------------------
        // Düğmeler
        // ------------------------------------------------------------------
        private void btnListele_Click(object sender, EventArgs e) { StartRun(true, false); }
        private void btnKopyala_Click(object sender, EventArgs e) { StartRun(false, true); }
        private void btnBasla_Click(object sender, EventArgs e) { StartRun(true, true); }
        private void btnHedefAc_Click(object sender, EventArgs e) { OpenInExplorer(AppConfig.TargetRoot); }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0) return;
            var it = listView1.SelectedItems[0];
            string path = Path.Combine(AppConfig.TargetRoot, SafePathSegment(it.Text), SafePathSegment(it.SubItems[1].Text));
            OpenInExplorer(path);
        }

        // ------------------------------------------------------------------
        // Çalıştırma (arka plan STA iş parçacığı; PDM COM nesneleri bu iş parçacığında yaratılır)
        // ------------------------------------------------------------------
        private void StartRun(bool doList, bool doCopy)
        {
            if (_configFatal) return;
            if (_busy)
            {
                if (!_isAuto) MessageBox.Show(this, "Bir işlem zaten sürüyor.", "Bekleyin", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (!doList && doCopy && _dtRapor.Rows.Count == 0)
            {
                _logger.Log("WARN", "COPY_ALL", "Liste boş; önce Listele çalıştırılmalı");
                if (!_isAuto) MessageBox.Show(this, "Önce 'Listele' ile iş emirlerini yükleyin.", "Kopyala", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DateTime from = dtpFrom.Value.Date, to = dtpTo.Value.Date;
            if (to < from) { var t = from; from = to; to = t; }

            SetBusy(true);
            var worker = new Thread(() =>
            {
                bool ok = false;
                bool copyStarted = false;
                try
                {
                    if (doList) DoList(from, to);
                    if (doCopy) { copyStarted = true; DoCopy(); }
                    ok = true;
                }
                catch (Exception ex)
                {
                    _logger.Log("FAIL", "CRASH", (copyStarted ? "Kopyalama" : "Listeleme") + " sırasında: " + ex);
                }
                finally
                {
                    _vault = null;
                    bool reportCopy = copyStarted;
                    SafePost(() => OnRunFinished(ok, reportCopy));
                }
            });
            worker.SetApartmentState(ApartmentState.STA);
            worker.IsBackground = true;
            worker.Name = "SyncWorker";
            worker.Start();
        }

        private void OnRunFinished(bool ok, bool didCopy)
        {
            SetBusy(false);

            if (didCopy)
            {
                string summary = "Kopyalandı: " + _runOk + ", Hatalı: " + _runFail + ", Excel: " + _runExcel;
                SetStatus((ok ? "Tamamlandı. " : "HATA ile kesildi. ") + summary);
                if (_isAuto)
                {
                    FinishAuto(!ok ? 1 : (_runFail > 0 ? 2 : 0), summary);
                    return;
                }
                MessageBox.Show(this,
                    (ok ? "Kopyalama tamamlandı.\n" : "Kopyalama hata ile kesildi.\n") + summary +
                    "\n\nAyrıntılar 'Çalışma Günlüğü' sekmesinde.",
                    "Kopyala", MessageBoxButtons.OK,
                    ok && _runFail == 0 ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            }
            else if (!ok)
            {
                SetStatus("Listeleme hata ile kesildi.");
                if (_isAuto) { FinishAuto(1, "LIST hatası"); return; }
                MessageBox.Show(this, "Listeleme hata ile kesildi. Ayrıntılar 'Çalışma Günlüğü' sekmesinde.",
                                "Listele", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FinishAuto(int exitCode, string summary)
        {
            if (_autoDone) return;
            _autoDone = true;
            if (_autoWatchdog != null) _autoWatchdog.Stop();
            Environment.ExitCode = exitCode;
            _logger.Finish(summary + " | ExitCode=" + exitCode);
            BeginInvoke(new Action(Close));
        }

        private void SetBusy(bool busy)
        {
            _busy = busy;
            bool enable = !busy && !_configFatal;
            btnListele.Enabled = btnKopyala.Enabled = btnBasla.Enabled = enable;
            dtpFrom.Enabled = dtpTo.Enabled = !busy;
            UseWaitCursor = busy;
            UpdateCurrentRunRow();
        }

        private void SetStatus(string text)
        {
            SafeInvoke(() => { lblStatus.Text = text; });
        }

        /// <summary>UI iş parçacığında eşzamanlı çalıştırır; form kapanmışsa sessizce atlar.</summary>
        private void SafeInvoke(Action a)
        {
            if (a == null) return;
            if (!InvokeRequired) { a(); return; }
            if (IsDisposed || !IsHandleCreated) return;
            try { Invoke(a); }
            catch (ObjectDisposedException) { }
            catch (InvalidOperationException) { }
        }

        /// <summary>UI iş parçacığına kuyruklar; çağıranı bekletmez.</summary>
        private void SafePost(Action a)
        {
            if (a == null || IsDisposed || !IsHandleCreated) return;
            try { BeginInvoke(a); }
            catch (ObjectDisposedException) { }
            catch (InvalidOperationException) { }
        }

        // ------------------------------------------------------------------
        // 1) LİSTELE — SAP'tan iş emirleri
        // ------------------------------------------------------------------
        private void DoList(DateTime from, DateTime toInclusive)
        {
            DateTime toExclusive = toInclusive.AddDays(1);
            _logger.Log("INFO", "DATE_RANGE", from.ToString("yyyy-MM-dd") + " .. " + toInclusive.ToString("yyyy-MM-dd"));
            SetStatus("SAP'tan iş emirleri okunuyor...");

            var dt = new DataTable();
            using (var conn = new SqlConnection(AppConfig.ConnectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(SQL_RAPOR, conn))
                {
                    cmd.CommandTimeout = 300;
                    cmd.Parameters.Add("@From", SqlDbType.DateTime).Value = from;
                    cmd.Parameters.Add("@To", SqlDbType.DateTime).Value = toExclusive;
                    using (var rdr = cmd.ExecuteReader()) dt.Load(rdr);
                }
            }

            _dtRapor = dt;
            _opCopyStatus.Clear();
            var groups = GroupProducts(dt).ToList();
            _lastGroups = groups.Count;

            SafeInvoke(() =>
            {
                listView1.BeginUpdate();
                listView1.Items.Clear();
                _itemsByKey.Clear();
                foreach (var g in groups)
                {
                    var first = g.First();
                    string project = S(first["Proje"]);
                    string subFolder = SubFolder(first);
                    string product = S(first["Üretilecek Ürün Kodu"]);
                    string opsJoin = string.Join("; ",
                        g.Select(r => S(r["Kaynak Kodu"])).Where(x => x.Length > 0).Distinct(StringComparer.OrdinalIgnoreCase));

                    var item = new ListViewItem(project);
                    item.SubItems.Add(subFolder);
                    item.SubItems.Add(product);
                    item.SubItems.Add(S(first["Durum"]));
                    item.SubItems.Add("Bekleniyor");
                    item.SubItems.Add(DateText(first["Başlangıç Tarihi"]));
                    item.SubItems.Add(S(first["Planlanan Miktar"]));
                    item.SubItems.Add(S(first["Üretilecek Ürün Adı"]));
                    item.SubItems.Add(opsJoin);
                    listView1.Items.Add(item);

                    _itemsByKey[project + "|" + subFolder + "|" + BaseCodeForSearch(product)] = item;
                }
                listView1.EndUpdate();
                progressBar1.Maximum = Math.Max(1, groups.Count);
                progressBar1.Value = 0;
            });

            _logger.Log("OK", "LIST", "Grup: " + groups.Count + ", Satır: " + dt.Rows.Count);
            SetStatus(groups.Count + " ürün grubu listelendi (" + dt.Rows.Count + " satır).");
        }

        // ------------------------------------------------------------------
        // 2) KOPYALA — PDM'den PDF + şablondan Excel
        // ------------------------------------------------------------------
        private void DoCopy()
        {
            var dt = _dtRapor;
            var folders = GroupFolders(dt).ToList();
            int totalProducts = folders.Sum(f => f.Select(r => S(r["Üretilecek Ürün Kodu"]))
                                                 .Where(x => x.Length > 0)
                                                 .Distinct(StringComparer.OrdinalIgnoreCase).Count());
            int done = 0;
            _runOk = 0; _runFail = 0; _runExcel = 0;
            _opCopyStatus.Clear();

            SafeInvoke(() => { progressBar1.Maximum = Math.Max(1, totalProducts); progressBar1.Value = 0; });
            _logger.Log("INFO", "COPY_ALL", "Klasör: " + folders.Count + ", Ürün: " + totalProducts, path: AppConfig.TargetRoot);

            bool templateOk = File.Exists(AppConfig.TemplateXlsx);
            if (!templateOk)
                _logger.Log("FAIL", "EXCEL", "Şablon bulunamadı, Excel üretilmeyecek: " + AppConfig.TemplateXlsx);

            // PDM'e bir kez bağlan; olmuyorsa her ürün için tekrar denemeden hızlı bitir.
            bool pdmOk = true;
            try { GetVault(); }
            catch (Exception ex)
            {
                pdmOk = false;
                _logger.Log("FAIL", "PDM", "Kasaya bağlanılamadı (" + AppConfig.PdmVault + "): " + ex.Message);
            }

            foreach (var folder in folders)
            {
                var first = folder.First();
                string project = S(first["Proje"]);
                string subFolder = SubFolder(first);
                string targetRoot = Path.Combine(AppConfig.TargetRoot, SafePathSegment(project), SafePathSegment(subFolder));

                try { Directory.CreateDirectory(targetRoot); }
                catch (Exception exFolder)
                {
                    _logger.Log("FAIL", "FOLDER", "Klasör oluşturulamadı: " + exFolder.Message, project, subFolder, null, null, targetRoot);
                    continue;
                }
                _logger.Log("INFO", "FOLDER", "İşleniyor", project, subFolder, null, null, targetRoot);

                // --- PDF kopyalama (tam ürün kodu ile; BOYA'da önce temiz kod, diğerlerinde önce tam kod) ---
                var productFullList = folder.Select(r => S(r["Üretilecek Ürün Kodu"]))
                                            .Where(x => x.Length > 0)
                                            .Distinct(StringComparer.OrdinalIgnoreCase)
                                            .ToList();

                foreach (var productFull in productFullList)
                {
                    var ops = folder.Where(r => S(r["Üretilecek Ürün Kodu"]).Equals(productFull, StringComparison.OrdinalIgnoreCase))
                                    .Select(r => S(r["Kaynak Kodu"]))
                                    .Where(x => x.Length > 0)
                                    .Distinct(StringComparer.OrdinalIgnoreCase)
                                    .ToList();

                    SetStatus("Kopyalanıyor (" + (done + 1) + "/" + totalProducts + "): " + project + " \\ " + subFolder + " \\ " + productFull);

                    string finalStatus = "Kopyalandı";
                    if (ops.Count == 0)
                    {
                        finalStatus = "Operasyon yok";
                        _logger.Log("WARN", "COPY_GROUP", "Kaynak kodu (operasyon) yok, PDF aranmadı", project, subFolder, productFull);
                    }

                    foreach (var op in ops)
                    {
                        string opFolder = Path.Combine(targetRoot, SafePathSegment(op));
                        string st;
                        string usedCode = productFull;

                        try { Directory.CreateDirectory(opFolder); st = null; }
                        catch (Exception exOp)
                        {
                            st = "Klasör hatası: " + exOp.Message;
                        }

                        if (st == null)
                        {
                            if (!pdmOk) st = "PDM bağlantısı yok";
                            else
                            {
                                st = "PDF bulunamadı";
                                foreach (var code in PdfSearchOrder(productFull, op))
                                {
                                    usedCode = code;
                                    st = FindAndCopyPdfFromPdm(code, opFolder, project, subFolder);
                                    if (IsCopied(st)) break;
                                }
                            }
                        }

                        if (!IsCopied(st)) finalStatus = st;
                        _opCopyStatus[Key(project, subFolder, productFull, op)] = st;
                        _logger.Log(IsCopied(st) ? "OK" : "FAIL", "COPY", st, project, subFolder, productFull, op,
                                    Path.Combine(opFolder, usedCode + ".pdf"));
                        if (IsCopied(st)) _runOk++; else _runFail++;
                    }

                    string key = project + "|" + subFolder + "|" + BaseCodeForSearch(productFull);
                    string rowStatus = finalStatus;
                    int doneNow = ++done;
                    SafeInvoke(() =>
                    {
                        ListViewItem li;
                        if (_itemsByKey.TryGetValue(key, out li))
                        {
                            li.SubItems[4].Text = rowStatus;
                            li.ForeColor = IsCopied(rowStatus) ? Color.Black : Color.Red;
                        }
                        progressBar1.Value = Math.Min(progressBar1.Maximum, doneNow);
                    });
                }

                // --- Excel (üst klasör + operasyon klasörleri), şablondan ---
                if (!templateOk) continue;
                WriteFolderExcels(folder, first, project, subFolder, targetRoot);
            }

            string summary = "Kopyalandı: " + _runOk + ", Hatalı: " + _runFail + ", Excel: " + _runExcel;
            _logger.Log("OK", "DONE", summary);
            SetStatus("Tamamlandı. " + summary);
        }

        private void WriteFolderExcels(IGrouping<string, DataRow> folder, DataRow first,
                                       string project, string subFolder, string targetRoot)
        {
            // BOYA'daki RAL -> baseCode eşlemesi (ilk görüleni al); GALVANIZ tespiti
            var boyaRalByBase = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var galvanizByBase = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (DataRow r in folder)
            {
                string op = S(r["Kaynak Kodu"]);
                string code = S(r["Üretilecek Ürün Kodu"]);
                if (code.Length == 0) continue;
                string baseCode = BaseCodeForSearch(code);

                if (Regex.IsMatch(code, @"-G$", RegexOptions.IgnoreCase) ||
                    (op.Length > 0 && op.IndexOf("GALVAN", StringComparison.OrdinalIgnoreCase) >= 0))
                    galvanizByBase.Add(baseCode);

                if (IsBoya(op))
                {
                    var m = Regex.Match(code, @"-RAL(\d{4})$", RegexOptions.IgnoreCase);
                    if (m.Success && !boyaRalByBase.ContainsKey(baseCode))
                        boyaRalByBase[baseCode] = "RAL" + m.Groups[1].Value;
                }
            }

            // Satırları mükerrer anahtarla teke indir (toplama/birleştirme yok)
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var excelRows = new List<ExcelRow>();
            foreach (DataRow r in folder)
            {
                var x = new ExcelRow
                {
                    Proje = S(r["Proje"]),
                    Klasor = SubFolder(r),
                    SapNo = S(r["SAP No"]),
                    HitsoftNo = S(r["Hitsoft No"]),
                    UrunKodu = S(r["Üretilecek Ürün Kodu"]),
                    Durum = S(r["Durum"]),
                    Miktar = S(r["Planlanan Miktar"]),
                    UrunAdi = S(r["Üretilecek Ürün Adı"]),
                    Operasyon = S(r["Kaynak Kodu"])
                };
                string dupKey = string.Join("|", x.Proje, x.Klasor, x.SapNo, x.HitsoftNo, x.UrunKodu, x.Miktar, x.UrunAdi, x.Operasyon);
                if (!seen.Add(dupKey)) continue;

                string st;
                x.DosyaDurumu = _opCopyStatus.TryGetValue(Key(x.Proje, x.Klasor, x.UrunKodu, x.Operasyon), out st) ? st : "İşlenmedi";

                // KAYNAK satırına BOYA RAL'ını (ya da GALVANIZ) taşı
                x.RalKoduForKaynak = "";
                if (x.Operasyon.IndexOf("KAYNAK", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    string baseCode = BaseCodeForSearch(x.UrunKodu);
                    string ral;
                    if (galvanizByBase.Contains(baseCode)) x.RalKoduForKaynak = "GALVANIZ";
                    else if (boyaRalByBase.TryGetValue(baseCode, out ral)) x.RalKoduForKaynak = ral;
                }
                excelRows.Add(x);
            }

            string tasarimci = S(first["Kullanıcı"]);

            // Üst klasör: Urunler.xlsx
            string topPath = Path.Combine(targetRoot, "Urunler.xlsx");
            try
            {
                var headerTop = new ExcelHeader { Proje = project, PozGrupNo = subFolder, Tasarimci = tasarimci, Operasyon = "GENEL", Tarih = DateTime.Today };
                WriteExcelFromTemplate(AppConfig.TemplateXlsx, topPath, headerTop, excelRows.Select(ToItem).ToList());
                _logger.Log("OK", "EXCEL", "Urunler.xlsx", project, subFolder, null, null, topPath);
                _runExcel++;
            }
            catch (Exception ex)
            {
                _logger.Log("FAIL", "EXCEL", "Şablondan yazım hatası: " + ex.Message, project, subFolder, null, null, topPath);
            }

            // Operasyon klasörleri: Urunler_<OP>.xlsx
            var byOp = excelRows.Where(x => x.Operasyon.Length > 0 && x.Operasyon != "-")
                                .GroupBy(x => x.Operasyon, StringComparer.OrdinalIgnoreCase);
            foreach (var g in byOp)
            {
                string op = g.Key;
                string opSafe = SafePathSegment(op);
                string opDir = Path.Combine(targetRoot, opSafe);
                string opPath = Path.Combine(opDir, "Urunler_" + opSafe + ".xlsx");
                try
                {
                    Directory.CreateDirectory(opDir);
                    var headerOp = new ExcelHeader { Proje = project, PozGrupNo = subFolder, Tasarimci = tasarimci, Operasyon = op, Tarih = DateTime.Today };
                    var itemsOp = g.OrderBy(x => x.Proje).ThenBy(x => x.Klasor).ThenBy(x => x.UrunKodu).Select(ToItem).ToList();
                    WriteExcelFromTemplate(AppConfig.TemplateXlsx, opPath, headerOp, itemsOp);
                    _logger.Log("OK", "EXCEL_OP", "Urunler_" + opSafe + ".xlsx", project, subFolder, null, op, opPath);
                    _runExcel++;
                }
                catch (Exception ex)
                {
                    _logger.Log("FAIL", "EXCEL_OP", "Şablondan yazım hatası: " + ex.Message, project, subFolder, null, op, opPath);
                }
            }
        }

        private static ExcelItem ToItem(ExcelRow x)
        {
            return new ExcelItem
            {
                HitsoftNo = x.HitsoftNo,
                UrunKodu = x.UrunKodu,
                UrunAdi = x.UrunAdi,
                Miktar = x.Miktar,                // metin olarak bırakılır
                RalKodu = x.RalKoduForKaynak,     // BOYA -> KAYNAK taşınan RAL / GALVANIZ
                Aciklama = ""
            };
        }

        // ------------------------------------------------------------------
        // PDM
        // ------------------------------------------------------------------
        private IEdmVault5 GetVault()
        {
            if (_vault == null)
            {
                IEdmVault5 v = new EdmVault5();
                if (!v.IsLoggedIn) v.LoginAuto(AppConfig.PdmVault, 0);
                _vault = v;
                _logger.Log("INFO", "PDM", "Kasaya bağlanıldı: " + AppConfig.PdmVault);
            }
            return _vault;
        }

        private string FindAndCopyPdfFromPdm(string productCode, string targetFolder, string projectHint, string subFolderHint)
        {
            string fileNameToFind = productCode + ".pdf";
            try
            {
                IEdmVault5 vault = GetVault();

                IEdmSearch5 search = (IEdmSearch5)vault.CreateSearch();
                search.FileName = fileNameToFind;
                search.FindFiles = true;
                search.FindFolders = false;

                // Adayları topla + puanla (proje/klasör adı yolda geçiyorsa öne al)
                var candidates = new List<(string path, int score)>();
                for (IEdmSearchResult5 res = search.GetFirstResult(); res != null; res = search.GetNextResult())
                {
                    string p = res.Path;
                    int s = 0;
                    if (!string.IsNullOrEmpty(projectHint) && p.IndexOf(projectHint, StringComparison.OrdinalIgnoreCase) >= 0) s += 10;
                    if (!string.IsNullOrEmpty(subFolderHint) && p.IndexOf(subFolderHint, StringComparison.OrdinalIgnoreCase) >= 0) s += 3;
                    s += Math.Min(5, p.Count(ch => ch == '\\'));
                    candidates.Add((p, s));
                }
                _logger.Log("INFO", "PDM", "Aday: " + candidates.Count, projectHint, subFolderHint, productCode);
                if (candidates.Count == 0) return "PDF bulunamadı";

                Directory.CreateDirectory(targetFolder);
                string outDir = targetFolder.TrimEnd('\\') + "\\";

                foreach (var cand in candidates.OrderByDescending(c => c.score).ThenByDescending(c => c.path.Length))
                {
                    IEdmFolder5 parent;
                    var file = (IEdmFile5)vault.GetFileFromPath(cand.path, out parent);
                    if (file == null) continue;

                    try
                    {
                        file.GetFileCopy(0, 0, outDir, 0, fileNameToFind);
                        string destPath = Path.Combine(outDir, fileNameToFind);
                        if (File.Exists(destPath)) return "Kopyalandı";
                        _logger.Log("WARN", "PDM_COPY", "GetFileCopy hata vermedi ama dosya oluşmadı", projectHint, subFolderHint, productCode, null, cand.path);
                    }
                    catch (COMException comEx)
                    {
                        _logger.Log("WARN", "PDM_COPY", "COM: " + comEx.Message, projectHint, subFolderHint, productCode, null, cand.path);
                    }
                    catch (Exception ex)
                    {
                        _logger.Log("WARN", "PDM_COPY", "EX: " + ex.Message, projectHint, subFolderHint, productCode, null, cand.path);
                    }
                }
                _logger.Log("FAIL", "PDM_COPY", "Tüm adaylar başarısız (" + candidates.Count + ")", projectHint, subFolderHint, productCode);
                return "İndirilemedi (GetFileCopy başarısız)";
            }
            catch (Exception ex)
            {
                _logger.Log("FAIL", "PDM", "Hata: " + ex.Message, projectHint, subFolderHint, productCode);
                return "Hata: " + ex.Message;
            }
        }

        // ------------------------------------------------------------------
        // Çalışma Günlüğü sekmesi
        // ------------------------------------------------------------------
        private void InitHistoryColumns()
        {
            lvRuns.Columns.Add("Başlangıç", 125);
            lvRuns.Columns.Add("Mod", 65);
            lvRuns.Columns.Add("Süre", 60);
            lvRuns.Columns.Add("Sonuç", 150);
            lvRuns.Columns.Add("Grup", 50, HorizontalAlignment.Right);
            lvRuns.Columns.Add("PDF OK", 60, HorizontalAlignment.Right);
            lvRuns.Columns.Add("PDF Hata", 65, HorizontalAlignment.Right);
            lvRuns.Columns.Add("Excel", 50, HorizontalAlignment.Right);
            lvRuns.Columns.Add("Uyarı", 50, HorizontalAlignment.Right);
            lvRuns.Columns.Add("Kullanıcı", 90);
            lvRuns.Columns.Add("Dosya", 330);

            lvLines.Columns.Add("Saat", 80);
            lvLines.Columns.Add("Seviye", 55);
            lvLines.Columns.Add("Adım", 85);
            lvLines.Columns.Add("Mesaj", 330);
            lvLines.Columns.Add("Proje", 100);
            lvLines.Columns.Add("Klasör", 90);
            lvLines.Columns.Add("Ürün Kodu", 140);
            lvLines.Columns.Add("Operasyon", 110);
            lvLines.Columns.Add("Dosya / Yol", 360);
        }

        private void RefreshRuns()
        {
            string keepFile = null;
            if (lvRuns.SelectedItems.Count > 0)
            {
                var sel = lvRuns.SelectedItems[0].Tag as RunSummary;
                if (sel != null) keepFile = sel.FilePath;
            }

            lvRuns.BeginUpdate();
            lvRuns.Items.Clear();
            if (_logger != null)
            {
                var cur = new ListViewItem(Enumerable.Repeat("", lvRuns.Columns.Count).ToArray());
                cur.Tag = null;
                cur.Font = new Font(lvRuns.Font, FontStyle.Bold);
                lvRuns.Items.Add(cur);
                UpdateCurrentRunRow();
            }

            // Günlük yedek klasöre düştüyse geçmiş de oradan okunur.
            string logDir = _logger != null ? _logger.LogDir : AppConfig.LogDir;
            List<RunSummary> runs;
            string info;
            try
            {
                runs = RunHistory.LoadSummaries(logDir, 200);
                info = runs.Count + " geçmiş çalışma — " + logDir;
            }
            catch (Exception ex)
            {
                runs = new List<RunSummary>();
                info = "Günlük klasörü okunamadı: " + ex.Message;
            }

            ListViewItem toSelect = null;
            foreach (var r in runs)
            {
                if (_logger != null && string.Equals(r.FilePath, _logger.TxtPath, StringComparison.OrdinalIgnoreCase)) continue;
                var it = new ListViewItem(new[]
                {
                    r.Started.ToString("dd.MM.yyyy HH:mm:ss"),
                    r.Mode,
                    r.Duration.HasValue ? FormatDuration(r.Duration.Value) : "",
                    r.Result,
                    r.Groups >= 0 ? r.Groups.ToString() : "",
                    r.CopiedOk.ToString(), r.CopyFail.ToString(), r.Excel.ToString(), r.Warns.ToString(),
                    r.User, r.FilePath
                });
                it.Tag = r;
                if (r.IsProblem) it.ForeColor = Color.Firebrick;
                else if (!r.HasCopy) it.ForeColor = Color.Gray;
                lvRuns.Items.Add(it);
                if (keepFile != null && string.Equals(r.FilePath, keepFile, StringComparison.OrdinalIgnoreCase)) toSelect = it;
            }
            lvRuns.EndUpdate();
            lblGunlukBilgi.Text = info;

            if (toSelect == null && lvRuns.Items.Count > 0) toSelect = lvRuns.Items[0];
            if (toSelect != null)
            {
                toSelect.Selected = true;   // -> lvRuns_SelectedIndexChanged -> LoadLines
                toSelect.Focused = true;
            }
            else
            {
                _shownLines = new List<LogEntry>();
                ApplyFilter();
            }
        }

        private void UpdateCurrentRunRow()
        {
            if (_logger == null || lvRuns.Items.Count == 0 || lvRuns.Items[0].Tag != null) return;
            var it = lvRuns.Items[0];
            bool problem = _logger.FailCopies > 0 || _logger.Fails > 0;
            string result = _busy ? "Çalışıyor..." : (problem ? "Hatalar var" : "Bu oturum");
            string[] v =
            {
                _logger.Started.ToString("dd.MM.yyyy HH:mm:ss"),
                _logger.Mode,
                FormatDuration(DateTime.Now - _logger.Started),
                result + " (canlı)",
                _lastGroups >= 0 ? _lastGroups.ToString() : "",
                _logger.OkCopies.ToString(), _logger.FailCopies.ToString(), _logger.ExcelCount.ToString(), _logger.Warns.ToString(),
                Environment.UserName, _logger.TxtPath
            };
            for (int i = 0; i < v.Length && i < it.SubItems.Count; i++) it.SubItems[i].Text = v[i];
            it.ForeColor = problem ? Color.Firebrick : Color.Black;
        }

        private static string FormatDuration(TimeSpan ts)
        {
            if (ts < TimeSpan.Zero) ts = TimeSpan.Zero;
            return ((int)ts.TotalMinutes) + ":" + ts.Seconds.ToString("00");
        }

        private void lvRuns_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadLines();
        }

        private void LoadLines()
        {
            if (lvRuns.SelectedItems.Count == 0)
            {
                _shownLines = new List<LogEntry>();
                ApplyFilter();
                return;
            }
            var run = lvRuns.SelectedItems[0].Tag as RunSummary;
            if (run == null)
            {
                _shownLines = _logger != null ? _logger.Snapshot() : new List<LogEntry>();
                _lastShownSeq = _shownLines.Count > 0 ? _shownLines[_shownLines.Count - 1].Seq : 0;
            }
            else
            {
                try { _shownLines = RunHistory.ReadEntries(run.FilePath); }
                catch (Exception ex)
                {
                    _shownLines = new List<LogEntry>
                    {
                        new LogEntry { Time = DateTime.Now, Level = "FAIL", Step = "READ", Message = "Dosya okunamadı: " + ex.Message, Path = run.FilePath }
                    };
                }
            }
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            lvLines.BeginUpdate();
            lvLines.Items.Clear();
            int shown = 0;
            foreach (var e in _shownLines)
            {
                if (!Matches(e)) continue;
                lvLines.Items.Add(MakeLineItem(e));
                if (++shown >= MaxShownLines) break;
            }
            lvLines.EndUpdate();
            lblSatirBilgi.Text = shown + " / " + _shownLines.Count + " satır" + (shown >= MaxShownLines ? " (ilk " + MaxShownLines + ")" : "");
            if (lvLines.Items.Count > 0 && lvRuns.SelectedItems.Count > 0 && lvRuns.SelectedItems[0].Tag == null)
                lvLines.Items[lvLines.Items.Count - 1].EnsureVisible();
        }

        private bool Matches(LogEntry e)
        {
            if (chkSadeceSorun.Checked && !e.IsProblem) return false;
            string f = txtFiltre.Text.Trim();
            if (f.Length == 0) return true;
            return Contains(e.Message, f) || Contains(e.Step, f) || Contains(e.Project, f) || Contains(e.Sub, f)
                || Contains(e.Code, f) || Contains(e.Op, f) || Contains(e.Path, f) || Contains(e.Level, f);
        }

        private static bool Contains(string s, string f)
        {
            return !string.IsNullOrEmpty(s) && s.IndexOf(f, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static ListViewItem MakeLineItem(LogEntry e)
        {
            var it = new ListViewItem(new[]
            {
                e.Time.ToString("HH:mm:ss"),
                e.Level ?? "", e.Step ?? "", e.Message ?? "",
                e.Project ?? "", e.Sub ?? "", e.Code ?? "", e.Op ?? "", e.Path ?? ""
            });
            it.Tag = e;
            switch (e.Level)
            {
                case "FAIL": it.ForeColor = Color.Firebrick; break;
                case "WARN": it.ForeColor = Color.DarkOrange; break;
                case "OK": it.ForeColor = Color.DarkGreen; break;
                default: it.ForeColor = Color.DimGray; break;
            }
            return it;
        }

        private void OnLogEntryFromAnyThread(LogEntry e)
        {
            if (IsDisposed || !IsHandleCreated) return;
            try { BeginInvoke(new Action<LogEntry>(OnLogEntry), e); }
            catch (ObjectDisposedException) { }
            catch (InvalidOperationException) { }
        }

        private void OnLogEntry(LogEntry e)
        {
            bool currentSelected = lvRuns.SelectedItems.Count > 0 && lvRuns.SelectedItems[0].Tag == null;
            if (currentSelected && e.Seq > _lastShownSeq)
            {
                _lastShownSeq = e.Seq;
                _shownLines.Add(e);
                if (Matches(e) && lvLines.Items.Count < MaxShownLines)
                {
                    var it = MakeLineItem(e);
                    lvLines.Items.Add(it);
                    if (tabControl1.SelectedTab == tabGunluk) it.EnsureVisible();
                    lblSatirBilgi.Text = lvLines.Items.Count + " / " + _shownLines.Count + " satır";
                }
            }
            UpdateCurrentRunRow();
        }

        private void btnGunlukYenile_Click(object sender, EventArgs e) { RefreshRuns(); }
        private void btnLogKlasoru_Click(object sender, EventArgs e) { OpenInExplorer(_logger != null ? _logger.LogDir : AppConfig.LogDir); }
        private void chkSadeceSorun_CheckedChanged(object sender, EventArgs e) { ApplyFilter(); }
        private void txtFiltre_TextChanged(object sender, EventArgs e) { ApplyFilter(); }

        private void btnLogDosyasi_Click(object sender, EventArgs e)
        {
            if (lvRuns.SelectedItems.Count == 0) return;
            var run = lvRuns.SelectedItems[0].Tag as RunSummary;
            string file = run != null ? run.FilePath : (_logger != null ? _logger.TxtPath : null);
            if (string.IsNullOrEmpty(file) || !File.Exists(file)) return;
            try { Process.Start("notepad.exe", "\"" + file + "\""); }
            catch (Exception ex) { MessageBox.Show(this, "Açılamadı: " + ex.Message); }
        }

        private void lvLines_DoubleClick(object sender, EventArgs e)
        {
            if (lvLines.SelectedItems.Count == 0) return;
            var entry = lvLines.SelectedItems[0].Tag as LogEntry;
            if (entry == null || string.IsNullOrEmpty(entry.Path)) return;
            if (File.Exists(entry.Path) || Directory.Exists(entry.Path)) OpenInExplorer(entry.Path);
        }

        private static void OpenInExplorer(string path)
        {
            try
            {
                if (File.Exists(path)) Process.Start("explorer.exe", "/select,\"" + path + "\"");
                else if (Directory.Exists(path)) Process.Start("explorer.exe", "\"" + path + "\"");
                else MessageBox.Show("Yol bulunamadı:\n" + path, "Aç", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Açılamadı: " + ex.Message, "Aç", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // ------------------------------------------------------------------
        // Yardımcılar (iş kuralları)
        // ------------------------------------------------------------------
        private static bool IsCopied(string st)
        {
            return !string.IsNullOrEmpty(st) && st.StartsWith("Kopyalandı", StringComparison.OrdinalIgnoreCase);
        }

        private static string Key(string project, string sub, string product, string op)
        {
            return project + "|" + sub + "|" + product + "|" + op;
        }

        private static bool IsBoya(string op)
        {
            return !string.IsNullOrWhiteSpace(op) && op.IndexOf("BOYA", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        /// <summary>BOYA için önce temiz kod (RAL/W/G/M atılmış), diğerlerinde önce tam kod denenir.</summary>
        private static IEnumerable<string> PdfSearchOrder(string fullCode, string operation)
        {
            string full = fullCode ?? "";
            string clean = BaseCodeForSearch(full);
            bool differs = !clean.Equals(full, StringComparison.OrdinalIgnoreCase);
            if (IsBoya(operation))
            {
                if (differs) yield return clean;
                yield return full;
            }
            else
            {
                yield return full;
                if (differs) yield return clean;
            }
        }

        private static string BaseCodeForSearch(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return "";
            // Sonda -RAL####, -W, -G, -M varsa at
            var m = Regex.Match(code, @"^(.*?)(?:-RAL\d{4}|-W|-G|-M)$", RegexOptions.IgnoreCase);
            return m.Success ? m.Groups[1].Value : code;
        }

        private static string S(object o)
        {
            return (o == null || o == DBNull.Value) ? "" : (Convert.ToString(o) ?? "").Trim();
        }

        private static string DateText(object o)
        {
            if (o == null || o == DBNull.Value) return "";
            try { return Convert.ToDateTime(o).ToShortDateString(); } catch { return S(o); }
        }

        private static object GetCol(DataRow row, params string[] names)
        {
            foreach (var n in names)
                if (row.Table.Columns.Contains(n)) return row[n];
            return null;
        }

        /// <summary>Klasör adı: "Pos No/Grup Kod" (eski adı "Sınıf Kodu") varsa o, yoksa SAP No.</summary>
        private static string SubFolder(DataRow row)
        {
            string groupNo = S(GetCol(row, "Pos No/Grup Kod", "Sınıf Kodu"));
            string posNo = S(row["SAP No"]);
            return groupNo.Length > 0 ? groupNo : posNo;
        }

        private static IEnumerable<IGrouping<string, DataRow>> GroupProducts(DataTable dt)
        {
            return dt.AsEnumerable().GroupBy(
                r => S(r["Proje"]) + "|" + SubFolder(r) + "|" + BaseCodeForSearch(S(r["Üretilecek Ürün Kodu"])),
                StringComparer.OrdinalIgnoreCase);
        }

        private static IEnumerable<IGrouping<string, DataRow>> GroupFolders(DataTable dt)
        {
            return dt.AsEnumerable().GroupBy(
                r => S(r["Proje"]) + "|" + SubFolder(r),
                StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Klasör adı olarak güvenli hale getirir: geçersiz karakterleri _, sondaki nokta/boşlukları kırpar,
        /// Windows rezerve adlarını (CON, PRN, NUL, COM1..) saptırır, boş kalırsa "_" döner.
        /// </summary>
        private static string SafePathSegment(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return "_";
            foreach (var c in Path.GetInvalidFileNameChars()) s = s.Replace(c, '_');
            s = s.Replace('/', '_').Replace('\\', '_');
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

        // ------------------------------------------------------------------
        // Excel
        // ------------------------------------------------------------------
        private sealed class ExcelRow
        {
            public string Proje, Klasor, SapNo, HitsoftNo, UrunKodu, Durum, DosyaDurumu, Miktar, UrunAdi, Operasyon, RalKoduForKaynak;
        }

        private sealed class ExcelHeader
        {
            public string Proje = "", PozGrupNo = "", Tasarimci = "", Operasyon = "";
            public DateTime Tarih = DateTime.Today;
        }

        private sealed class ExcelItem
        {
            public string HitsoftNo = "", UrunKodu = "", UrunAdi = "", Miktar = "", RalKodu = "", Aciklama = "";
        }

        private static void WriteExcelFromTemplate(string templatePath, string outPath, ExcelHeader h, List<ExcelItem> items)
        {
            var outDir = Path.GetDirectoryName(outPath);
            if (!string.IsNullOrEmpty(outDir)) Directory.CreateDirectory(outDir);

            using (var wb = new XLWorkbook(templatePath))
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
                ws.Range("A7:G10000").Clear(XLClearOptions.Contents);

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

        // ------------------------------------------------------------------
        // SQL
        // ------------------------------------------------------------------
        private const string SQL_RAPOR = @"
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
    }
}
