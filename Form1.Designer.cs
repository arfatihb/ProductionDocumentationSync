namespace ProductionDocumentationSync
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabIslemler = new System.Windows.Forms.TabPage();
            this.listView1 = new System.Windows.Forms.ListView();
            this.Proje = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.Klasör = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.Ürün_Kodu = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.Durum = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.Dosya_Durumu = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.BaslangicTarihi = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.PlanlananMiktar = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.UrunAdi = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.KaynakKodu = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.flowTop = new System.Windows.Forms.FlowLayoutPanel();
            this.lblTarih = new System.Windows.Forms.Label();
            this.dtpFrom = new System.Windows.Forms.DateTimePicker();
            this.lblTire = new System.Windows.Forms.Label();
            this.dtpTo = new System.Windows.Forms.DateTimePicker();
            this.btnListele = new System.Windows.Forms.Button();
            this.btnKopyala = new System.Windows.Forms.Button();
            this.btnBasla = new System.Windows.Forms.Button();
            this.btnHedefAc = new System.Windows.Forms.Button();
            this.panelBottom = new System.Windows.Forms.Panel();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.tabGunluk = new System.Windows.Forms.TabPage();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.lvRuns = new System.Windows.Forms.ListView();
            this.flowRunsTop = new System.Windows.Forms.FlowLayoutPanel();
            this.btnGunlukYenile = new System.Windows.Forms.Button();
            this.btnLogDosyasi = new System.Windows.Forms.Button();
            this.btnLogKlasoru = new System.Windows.Forms.Button();
            this.lblGunlukBilgi = new System.Windows.Forms.Label();
            this.lvLines = new System.Windows.Forms.ListView();
            this.flowLinesTop = new System.Windows.Forms.FlowLayoutPanel();
            this.chkSadeceSorun = new System.Windows.Forms.CheckBox();
            this.lblFiltre = new System.Windows.Forms.Label();
            this.txtFiltre = new System.Windows.Forms.TextBox();
            this.lblSatirBilgi = new System.Windows.Forms.Label();
            this.tabZamanlama = new System.Windows.Forms.TabPage();
            this.grpGorevDurum = new System.Windows.Forms.GroupBox();
            this.lblGorevDurum = new System.Windows.Forms.Label();
            this.lblGorevSonraki = new System.Windows.Forms.Label();
            this.lblGorevSon = new System.Windows.Forms.Label();
            this.lblGorevProgram = new System.Windows.Forms.Label();
            this.grpGorevAyar = new System.Windows.Forms.GroupBox();
            this.lblGorevSaat = new System.Windows.Forms.Label();
            this.dtpGorevSaat = new System.Windows.Forms.DateTimePicker();
            this.lblGorevGunler = new System.Windows.Forms.Label();
            this.chkGun1 = new System.Windows.Forms.CheckBox();
            this.chkGun2 = new System.Windows.Forms.CheckBox();
            this.chkGun3 = new System.Windows.Forms.CheckBox();
            this.chkGun4 = new System.Windows.Forms.CheckBox();
            this.chkGun5 = new System.Windows.Forms.CheckBox();
            this.chkGun6 = new System.Windows.Forms.CheckBox();
            this.chkGun7 = new System.Windows.Forms.CheckBox();
            this.btnGorevKur = new System.Windows.Forms.Button();
            this.btnGorevKaldir = new System.Windows.Forms.Button();
            this.btnGorevSimdi = new System.Windows.Forms.Button();
            this.btnGorevYenile = new System.Windows.Forms.Button();
            this.btnGorevZamanlayici = new System.Windows.Forms.Button();
            this.lblGorevNot = new System.Windows.Forms.Label();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.lblStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblHedef = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblMode = new System.Windows.Forms.ToolStripStatusLabel();
            this.tabControl1.SuspendLayout();
            this.tabIslemler.SuspendLayout();
            this.flowTop.SuspendLayout();
            this.panelBottom.SuspendLayout();
            this.tabGunluk.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.flowRunsTop.SuspendLayout();
            this.flowLinesTop.SuspendLayout();
            this.tabZamanlama.SuspendLayout();
            this.grpGorevDurum.SuspendLayout();
            this.grpGorevAyar.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            //
            // tabControl1
            //
            this.tabControl1.Controls.Add(this.tabIslemler);
            this.tabControl1.Controls.Add(this.tabGunluk);
            this.tabControl1.Controls.Add(this.tabZamanlama);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1134, 619);
            this.tabControl1.TabIndex = 0;
            //
            // tabIslemler
            //
            this.tabIslemler.Controls.Add(this.listView1);
            this.tabIslemler.Controls.Add(this.flowTop);
            this.tabIslemler.Controls.Add(this.panelBottom);
            this.tabIslemler.Location = new System.Drawing.Point(4, 22);
            this.tabIslemler.Name = "tabIslemler";
            this.tabIslemler.Padding = new System.Windows.Forms.Padding(3);
            this.tabIslemler.Size = new System.Drawing.Size(1126, 593);
            this.tabIslemler.TabIndex = 0;
            this.tabIslemler.Text = "İşlemler";
            this.tabIslemler.UseVisualStyleBackColor = true;
            //
            // listView1
            //
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.Proje,
            this.Klasör,
            this.Ürün_Kodu,
            this.Durum,
            this.Dosya_Durumu,
            this.BaslangicTarihi,
            this.PlanlananMiktar,
            this.UrunAdi,
            this.KaynakKodu});
            this.listView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView1.FullRowSelect = true;
            this.listView1.GridLines = true;
            this.listView1.HideSelection = false;
            this.listView1.Location = new System.Drawing.Point(3, 39);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(1120, 523);
            this.listView1.TabIndex = 1;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            this.listView1.DoubleClick += new System.EventHandler(this.listView1_DoubleClick);
            this.listView1.Resize += new System.EventHandler(this.listView1_Resize);
            //
            // Proje
            //
            this.Proje.Text = "Proje";
            this.Proje.Width = 110;
            //
            // Klasör
            //
            this.Klasör.Text = "Klasör";
            this.Klasör.Width = 110;
            //
            // Ürün_Kodu
            //
            this.Ürün_Kodu.Text = "Ürün Kodu";
            this.Ürün_Kodu.Width = 140;
            //
            // Durum
            //
            this.Durum.Text = "Durum";
            this.Durum.Width = 90;
            //
            // Dosya_Durumu
            //
            this.Dosya_Durumu.Text = "Dosya Durumu";
            this.Dosya_Durumu.Width = 130;
            //
            // BaslangicTarihi
            //
            this.BaslangicTarihi.Text = "Başlangıç";
            this.BaslangicTarihi.Width = 90;
            //
            // PlanlananMiktar
            //
            this.PlanlananMiktar.Text = "Miktar";
            this.PlanlananMiktar.Width = 60;
            //
            // UrunAdi
            //
            this.UrunAdi.Text = "Ürün Adı";
            this.UrunAdi.Width = 200;
            //
            // KaynakKodu
            //
            this.KaynakKodu.Text = "Kaynak Kodu";
            this.KaynakKodu.Width = 140;
            //
            // flowTop
            //
            this.flowTop.Controls.Add(this.lblTarih);
            this.flowTop.Controls.Add(this.dtpFrom);
            this.flowTop.Controls.Add(this.lblTire);
            this.flowTop.Controls.Add(this.dtpTo);
            this.flowTop.Controls.Add(this.btnListele);
            this.flowTop.Controls.Add(this.btnKopyala);
            this.flowTop.Controls.Add(this.btnBasla);
            this.flowTop.Controls.Add(this.btnHedefAc);
            this.flowTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.flowTop.Location = new System.Drawing.Point(3, 3);
            this.flowTop.Name = "flowTop";
            this.flowTop.Padding = new System.Windows.Forms.Padding(3, 2, 3, 0);
            this.flowTop.Size = new System.Drawing.Size(1120, 36);
            this.flowTop.TabIndex = 0;
            this.flowTop.WrapContents = false;
            //
            // lblTarih
            //
            this.lblTarih.AutoSize = true;
            this.lblTarih.Location = new System.Drawing.Point(6, 10);
            this.lblTarih.Margin = new System.Windows.Forms.Padding(3, 8, 3, 0);
            this.lblTarih.Name = "lblTarih";
            this.lblTarih.Size = new System.Drawing.Size(81, 13);
            this.lblTarih.TabIndex = 0;
            this.lblTarih.Text = "Başlangıç tarihi:";
            //
            // dtpFrom
            //
            this.dtpFrom.CustomFormat = "dd.MM.yyyy";
            this.dtpFrom.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpFrom.Location = new System.Drawing.Point(93, 7);
            this.dtpFrom.Margin = new System.Windows.Forms.Padding(3, 5, 3, 3);
            this.dtpFrom.Name = "dtpFrom";
            this.dtpFrom.Size = new System.Drawing.Size(105, 20);
            this.dtpFrom.TabIndex = 1;
            //
            // lblTire
            //
            this.lblTire.AutoSize = true;
            this.lblTire.Location = new System.Drawing.Point(201, 10);
            this.lblTire.Margin = new System.Windows.Forms.Padding(0, 8, 0, 0);
            this.lblTire.Name = "lblTire";
            this.lblTire.Size = new System.Drawing.Size(10, 13);
            this.lblTire.TabIndex = 2;
            this.lblTire.Text = "-";
            //
            // dtpTo
            //
            this.dtpTo.CustomFormat = "dd.MM.yyyy";
            this.dtpTo.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpTo.Location = new System.Drawing.Point(214, 7);
            this.dtpTo.Margin = new System.Windows.Forms.Padding(3, 5, 3, 3);
            this.dtpTo.Name = "dtpTo";
            this.dtpTo.Size = new System.Drawing.Size(105, 20);
            this.dtpTo.TabIndex = 3;
            //
            // btnListele
            //
            this.btnListele.Location = new System.Drawing.Point(334, 5);
            this.btnListele.Margin = new System.Windows.Forms.Padding(12, 3, 3, 3);
            this.btnListele.Name = "btnListele";
            this.btnListele.Size = new System.Drawing.Size(85, 25);
            this.btnListele.TabIndex = 4;
            this.btnListele.Text = "Listele";
            this.btnListele.UseVisualStyleBackColor = true;
            this.btnListele.Click += new System.EventHandler(this.btnListele_Click);
            //
            // btnKopyala
            //
            this.btnKopyala.Location = new System.Drawing.Point(425, 5);
            this.btnKopyala.Name = "btnKopyala";
            this.btnKopyala.Size = new System.Drawing.Size(85, 25);
            this.btnKopyala.TabIndex = 5;
            this.btnKopyala.Text = "Kopyala";
            this.btnKopyala.UseVisualStyleBackColor = true;
            this.btnKopyala.Click += new System.EventHandler(this.btnKopyala_Click);
            //
            // btnBasla
            //
            this.btnBasla.Location = new System.Drawing.Point(516, 5);
            this.btnBasla.Name = "btnBasla";
            this.btnBasla.Size = new System.Drawing.Size(140, 25);
            this.btnBasla.TabIndex = 6;
            this.btnBasla.Text = "Listele + Kopyala";
            this.btnBasla.UseVisualStyleBackColor = true;
            this.btnBasla.Click += new System.EventHandler(this.btnBasla_Click);
            //
            // btnHedefAc
            //
            this.btnHedefAc.Location = new System.Drawing.Point(671, 5);
            this.btnHedefAc.Margin = new System.Windows.Forms.Padding(12, 3, 3, 3);
            this.btnHedefAc.Name = "btnHedefAc";
            this.btnHedefAc.Size = new System.Drawing.Size(125, 25);
            this.btnHedefAc.TabIndex = 7;
            this.btnHedefAc.Text = "Hedef Klasörü Aç";
            this.btnHedefAc.UseVisualStyleBackColor = true;
            this.btnHedefAc.Click += new System.EventHandler(this.btnHedefAc_Click);
            //
            // panelBottom
            //
            this.panelBottom.Controls.Add(this.progressBar1);
            this.panelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelBottom.Location = new System.Drawing.Point(3, 562);
            this.panelBottom.Name = "panelBottom";
            this.panelBottom.Padding = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this.panelBottom.Size = new System.Drawing.Size(1120, 28);
            this.panelBottom.TabIndex = 2;
            //
            // progressBar1
            //
            this.progressBar1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.progressBar1.Location = new System.Drawing.Point(0, 4);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(1120, 24);
            this.progressBar1.TabIndex = 0;
            //
            // tabGunluk
            //
            this.tabGunluk.Controls.Add(this.splitContainer1);
            this.tabGunluk.Location = new System.Drawing.Point(4, 22);
            this.tabGunluk.Name = "tabGunluk";
            this.tabGunluk.Padding = new System.Windows.Forms.Padding(3);
            this.tabGunluk.Size = new System.Drawing.Size(1126, 593);
            this.tabGunluk.TabIndex = 1;
            this.tabGunluk.Text = "Çalışma Günlüğü";
            this.tabGunluk.UseVisualStyleBackColor = true;
            //
            // splitContainer1
            //
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(3, 3);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            //
            // splitContainer1.Panel1
            //
            this.splitContainer1.Panel1.Controls.Add(this.lvRuns);
            this.splitContainer1.Panel1.Controls.Add(this.flowRunsTop);
            //
            // splitContainer1.Panel2
            //
            this.splitContainer1.Panel2.Controls.Add(this.lvLines);
            this.splitContainer1.Panel2.Controls.Add(this.flowLinesTop);
            this.splitContainer1.Size = new System.Drawing.Size(1120, 587);
            this.splitContainer1.SplitterDistance = 215;
            this.splitContainer1.TabIndex = 0;
            //
            // lvRuns
            //
            this.lvRuns.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvRuns.FullRowSelect = true;
            this.lvRuns.GridLines = true;
            this.lvRuns.HideSelection = false;
            this.lvRuns.Location = new System.Drawing.Point(0, 32);
            this.lvRuns.MultiSelect = false;
            this.lvRuns.Name = "lvRuns";
            this.lvRuns.Size = new System.Drawing.Size(1120, 183);
            this.lvRuns.TabIndex = 1;
            this.lvRuns.UseCompatibleStateImageBehavior = false;
            this.lvRuns.View = System.Windows.Forms.View.Details;
            this.lvRuns.SelectedIndexChanged += new System.EventHandler(this.lvRuns_SelectedIndexChanged);
            //
            // flowRunsTop
            //
            this.flowRunsTop.Controls.Add(this.btnGunlukYenile);
            this.flowRunsTop.Controls.Add(this.btnLogDosyasi);
            this.flowRunsTop.Controls.Add(this.btnLogKlasoru);
            this.flowRunsTop.Controls.Add(this.lblGunlukBilgi);
            this.flowRunsTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.flowRunsTop.Location = new System.Drawing.Point(0, 0);
            this.flowRunsTop.Name = "flowRunsTop";
            this.flowRunsTop.Padding = new System.Windows.Forms.Padding(0, 2, 0, 0);
            this.flowRunsTop.Size = new System.Drawing.Size(1120, 32);
            this.flowRunsTop.TabIndex = 0;
            this.flowRunsTop.WrapContents = false;
            //
            // btnGunlukYenile
            //
            this.btnGunlukYenile.Location = new System.Drawing.Point(3, 5);
            this.btnGunlukYenile.Name = "btnGunlukYenile";
            this.btnGunlukYenile.Size = new System.Drawing.Size(75, 24);
            this.btnGunlukYenile.TabIndex = 0;
            this.btnGunlukYenile.Text = "Yenile";
            this.btnGunlukYenile.UseVisualStyleBackColor = true;
            this.btnGunlukYenile.Click += new System.EventHandler(this.btnGunlukYenile_Click);
            //
            // btnLogDosyasi
            //
            this.btnLogDosyasi.Location = new System.Drawing.Point(84, 5);
            this.btnLogDosyasi.Name = "btnLogDosyasi";
            this.btnLogDosyasi.Size = new System.Drawing.Size(140, 24);
            this.btnLogDosyasi.TabIndex = 1;
            this.btnLogDosyasi.Text = "Günlük Dosyasını Aç";
            this.btnLogDosyasi.UseVisualStyleBackColor = true;
            this.btnLogDosyasi.Click += new System.EventHandler(this.btnLogDosyasi_Click);
            //
            // btnLogKlasoru
            //
            this.btnLogKlasoru.Location = new System.Drawing.Point(230, 5);
            this.btnLogKlasoru.Name = "btnLogKlasoru";
            this.btnLogKlasoru.Size = new System.Drawing.Size(140, 24);
            this.btnLogKlasoru.TabIndex = 2;
            this.btnLogKlasoru.Text = "Günlük Klasörünü Aç";
            this.btnLogKlasoru.UseVisualStyleBackColor = true;
            this.btnLogKlasoru.Click += new System.EventHandler(this.btnLogKlasoru_Click);
            //
            // lblGunlukBilgi
            //
            this.lblGunlukBilgi.AutoSize = true;
            this.lblGunlukBilgi.ForeColor = System.Drawing.SystemColors.GrayText;
            this.lblGunlukBilgi.Location = new System.Drawing.Point(385, 10);
            this.lblGunlukBilgi.Margin = new System.Windows.Forms.Padding(12, 8, 3, 0);
            this.lblGunlukBilgi.Name = "lblGunlukBilgi";
            this.lblGunlukBilgi.Size = new System.Drawing.Size(0, 13);
            this.lblGunlukBilgi.TabIndex = 3;
            //
            // lvLines
            //
            this.lvLines.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvLines.FullRowSelect = true;
            this.lvLines.GridLines = true;
            this.lvLines.HideSelection = false;
            this.lvLines.Location = new System.Drawing.Point(0, 32);
            this.lvLines.Name = "lvLines";
            this.lvLines.Size = new System.Drawing.Size(1120, 336);
            this.lvLines.TabIndex = 1;
            this.lvLines.UseCompatibleStateImageBehavior = false;
            this.lvLines.View = System.Windows.Forms.View.Details;
            this.lvLines.DoubleClick += new System.EventHandler(this.lvLines_DoubleClick);
            //
            // flowLinesTop
            //
            this.flowLinesTop.Controls.Add(this.chkSadeceSorun);
            this.flowLinesTop.Controls.Add(this.lblFiltre);
            this.flowLinesTop.Controls.Add(this.txtFiltre);
            this.flowLinesTop.Controls.Add(this.lblSatirBilgi);
            this.flowLinesTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.flowLinesTop.Location = new System.Drawing.Point(0, 0);
            this.flowLinesTop.Name = "flowLinesTop";
            this.flowLinesTop.Padding = new System.Windows.Forms.Padding(0, 2, 0, 0);
            this.flowLinesTop.Size = new System.Drawing.Size(1120, 32);
            this.flowLinesTop.TabIndex = 0;
            this.flowLinesTop.WrapContents = false;
            //
            // chkSadeceSorun
            //
            this.chkSadeceSorun.AutoSize = true;
            this.chkSadeceSorun.Location = new System.Drawing.Point(6, 9);
            this.chkSadeceSorun.Margin = new System.Windows.Forms.Padding(6, 7, 3, 0);
            this.chkSadeceSorun.Name = "chkSadeceSorun";
            this.chkSadeceSorun.Size = new System.Drawing.Size(123, 17);
            this.chkSadeceSorun.TabIndex = 0;
            this.chkSadeceSorun.Text = "Sadece hata / uyarı";
            this.chkSadeceSorun.UseVisualStyleBackColor = true;
            this.chkSadeceSorun.CheckedChanged += new System.EventHandler(this.chkSadeceSorun_CheckedChanged);
            //
            // lblFiltre
            //
            this.lblFiltre.AutoSize = true;
            this.lblFiltre.Location = new System.Drawing.Point(144, 10);
            this.lblFiltre.Margin = new System.Windows.Forms.Padding(12, 8, 3, 0);
            this.lblFiltre.Name = "lblFiltre";
            this.lblFiltre.Size = new System.Drawing.Size(26, 13);
            this.lblFiltre.TabIndex = 1;
            this.lblFiltre.Text = "Ara:";
            //
            // txtFiltre
            //
            this.txtFiltre.Location = new System.Drawing.Point(176, 7);
            this.txtFiltre.Margin = new System.Windows.Forms.Padding(3, 5, 3, 3);
            this.txtFiltre.Name = "txtFiltre";
            this.txtFiltre.Size = new System.Drawing.Size(220, 20);
            this.txtFiltre.TabIndex = 2;
            this.txtFiltre.TextChanged += new System.EventHandler(this.txtFiltre_TextChanged);
            //
            // lblSatirBilgi
            //
            this.lblSatirBilgi.AutoSize = true;
            this.lblSatirBilgi.ForeColor = System.Drawing.SystemColors.GrayText;
            this.lblSatirBilgi.Location = new System.Drawing.Point(411, 10);
            this.lblSatirBilgi.Margin = new System.Windows.Forms.Padding(12, 8, 3, 0);
            this.lblSatirBilgi.Name = "lblSatirBilgi";
            this.lblSatirBilgi.Size = new System.Drawing.Size(0, 13);
            this.lblSatirBilgi.TabIndex = 3;
            //
            // tabZamanlama
            //
            this.tabZamanlama.Controls.Add(this.grpGorevDurum);
            this.tabZamanlama.Controls.Add(this.grpGorevAyar);
            this.tabZamanlama.Controls.Add(this.lblGorevNot);
            this.tabZamanlama.Location = new System.Drawing.Point(4, 22);
            this.tabZamanlama.Name = "tabZamanlama";
            this.tabZamanlama.Padding = new System.Windows.Forms.Padding(3);
            this.tabZamanlama.Size = new System.Drawing.Size(1126, 593);
            this.tabZamanlama.TabIndex = 2;
            this.tabZamanlama.Text = "Zamanlama";
            this.tabZamanlama.UseVisualStyleBackColor = true;
            //
            // grpGorevDurum
            //
            this.grpGorevDurum.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpGorevDurum.Controls.Add(this.lblGorevDurum);
            this.grpGorevDurum.Controls.Add(this.lblGorevSonraki);
            this.grpGorevDurum.Controls.Add(this.lblGorevSon);
            this.grpGorevDurum.Controls.Add(this.lblGorevProgram);
            this.grpGorevDurum.Location = new System.Drawing.Point(12, 12);
            this.grpGorevDurum.Name = "grpGorevDurum";
            this.grpGorevDurum.Size = new System.Drawing.Size(1100, 118);
            this.grpGorevDurum.TabIndex = 0;
            this.grpGorevDurum.TabStop = false;
            this.grpGorevDurum.Text = "Zamanlanmış görev (Windows Görev Zamanlayıcı)";
            //
            // lblGorevDurum
            //
            this.lblGorevDurum.AutoSize = true;
            this.lblGorevDurum.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold);
            this.lblGorevDurum.Location = new System.Drawing.Point(12, 24);
            this.lblGorevDurum.Name = "lblGorevDurum";
            this.lblGorevDurum.Size = new System.Drawing.Size(190, 16);
            this.lblGorevDurum.TabIndex = 0;
            this.lblGorevDurum.Text = "Görev durumu sorgulanıyor...";
            //
            // lblGorevSonraki
            //
            this.lblGorevSonraki.AutoSize = true;
            this.lblGorevSonraki.Location = new System.Drawing.Point(12, 50);
            this.lblGorevSonraki.Name = "lblGorevSonraki";
            this.lblGorevSonraki.Size = new System.Drawing.Size(100, 13);
            this.lblGorevSonraki.TabIndex = 1;
            this.lblGorevSonraki.Text = "Sonraki çalışma: -";
            //
            // lblGorevSon
            //
            this.lblGorevSon.AutoSize = true;
            this.lblGorevSon.Location = new System.Drawing.Point(12, 70);
            this.lblGorevSon.Name = "lblGorevSon";
            this.lblGorevSon.Size = new System.Drawing.Size(85, 13);
            this.lblGorevSon.TabIndex = 2;
            this.lblGorevSon.Text = "Son çalışma: -";
            //
            // lblGorevProgram
            //
            this.lblGorevProgram.AutoSize = true;
            this.lblGorevProgram.Location = new System.Drawing.Point(12, 90);
            this.lblGorevProgram.Name = "lblGorevProgram";
            this.lblGorevProgram.Size = new System.Drawing.Size(60, 13);
            this.lblGorevProgram.TabIndex = 3;
            this.lblGorevProgram.Text = "Program: -";
            //
            // grpGorevAyar
            //
            this.grpGorevAyar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpGorevAyar.Controls.Add(this.lblGorevSaat);
            this.grpGorevAyar.Controls.Add(this.dtpGorevSaat);
            this.grpGorevAyar.Controls.Add(this.lblGorevGunler);
            this.grpGorevAyar.Controls.Add(this.chkGun1);
            this.grpGorevAyar.Controls.Add(this.chkGun2);
            this.grpGorevAyar.Controls.Add(this.chkGun3);
            this.grpGorevAyar.Controls.Add(this.chkGun4);
            this.grpGorevAyar.Controls.Add(this.chkGun5);
            this.grpGorevAyar.Controls.Add(this.chkGun6);
            this.grpGorevAyar.Controls.Add(this.chkGun7);
            this.grpGorevAyar.Controls.Add(this.btnGorevKur);
            this.grpGorevAyar.Controls.Add(this.btnGorevKaldir);
            this.grpGorevAyar.Controls.Add(this.btnGorevSimdi);
            this.grpGorevAyar.Controls.Add(this.btnGorevYenile);
            this.grpGorevAyar.Controls.Add(this.btnGorevZamanlayici);
            this.grpGorevAyar.Location = new System.Drawing.Point(12, 142);
            this.grpGorevAyar.Name = "grpGorevAyar";
            this.grpGorevAyar.Size = new System.Drawing.Size(1100, 135);
            this.grpGorevAyar.TabIndex = 1;
            this.grpGorevAyar.TabStop = false;
            this.grpGorevAyar.Text = "Ayarla";
            //
            // lblGorevSaat
            //
            this.lblGorevSaat.AutoSize = true;
            this.lblGorevSaat.Location = new System.Drawing.Point(12, 30);
            this.lblGorevSaat.Name = "lblGorevSaat";
            this.lblGorevSaat.Size = new System.Drawing.Size(76, 13);
            this.lblGorevSaat.TabIndex = 0;
            this.lblGorevSaat.Text = "Çalışma saati:";
            //
            // dtpGorevSaat
            //
            this.dtpGorevSaat.CustomFormat = "HH:mm";
            this.dtpGorevSaat.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpGorevSaat.Location = new System.Drawing.Point(100, 26);
            this.dtpGorevSaat.Name = "dtpGorevSaat";
            this.dtpGorevSaat.ShowUpDown = true;
            this.dtpGorevSaat.Size = new System.Drawing.Size(70, 20);
            this.dtpGorevSaat.TabIndex = 1;
            this.dtpGorevSaat.Value = new System.DateTime(2026, 1, 1, 7, 0, 0, 0);
            //
            // lblGorevGunler
            //
            this.lblGorevGunler.AutoSize = true;
            this.lblGorevGunler.Location = new System.Drawing.Point(12, 62);
            this.lblGorevGunler.Name = "lblGorevGunler";
            this.lblGorevGunler.Size = new System.Drawing.Size(42, 13);
            this.lblGorevGunler.TabIndex = 2;
            this.lblGorevGunler.Text = "Günler:";
            //
            // chkGun1..7
            //
            this.chkGun1.AutoSize = true;
            this.chkGun1.Checked = true;
            this.chkGun1.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkGun1.Location = new System.Drawing.Point(100, 61);
            this.chkGun1.Name = "chkGun1";
            this.chkGun1.Size = new System.Drawing.Size(44, 17);
            this.chkGun1.TabIndex = 3;
            this.chkGun1.Text = "Pzt";
            this.chkGun1.UseVisualStyleBackColor = true;
            this.chkGun2.AutoSize = true;
            this.chkGun2.Checked = true;
            this.chkGun2.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkGun2.Location = new System.Drawing.Point(160, 61);
            this.chkGun2.Name = "chkGun2";
            this.chkGun2.Size = new System.Drawing.Size(43, 17);
            this.chkGun2.TabIndex = 4;
            this.chkGun2.Text = "Sal";
            this.chkGun2.UseVisualStyleBackColor = true;
            this.chkGun3.AutoSize = true;
            this.chkGun3.Checked = true;
            this.chkGun3.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkGun3.Location = new System.Drawing.Point(220, 61);
            this.chkGun3.Name = "chkGun3";
            this.chkGun3.Size = new System.Drawing.Size(44, 17);
            this.chkGun3.TabIndex = 5;
            this.chkGun3.Text = "Çar";
            this.chkGun3.UseVisualStyleBackColor = true;
            this.chkGun4.AutoSize = true;
            this.chkGun4.Checked = true;
            this.chkGun4.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkGun4.Location = new System.Drawing.Point(280, 61);
            this.chkGun4.Name = "chkGun4";
            this.chkGun4.Size = new System.Drawing.Size(44, 17);
            this.chkGun4.TabIndex = 6;
            this.chkGun4.Text = "Per";
            this.chkGun4.UseVisualStyleBackColor = true;
            this.chkGun5.AutoSize = true;
            this.chkGun5.Checked = true;
            this.chkGun5.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkGun5.Location = new System.Drawing.Point(340, 61);
            this.chkGun5.Name = "chkGun5";
            this.chkGun5.Size = new System.Drawing.Size(47, 17);
            this.chkGun5.TabIndex = 7;
            this.chkGun5.Text = "Cum";
            this.chkGun5.UseVisualStyleBackColor = true;
            this.chkGun6.AutoSize = true;
            this.chkGun6.Checked = true;
            this.chkGun6.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkGun6.Location = new System.Drawing.Point(400, 61);
            this.chkGun6.Name = "chkGun6";
            this.chkGun6.Size = new System.Drawing.Size(46, 17);
            this.chkGun6.TabIndex = 8;
            this.chkGun6.Text = "Cmt";
            this.chkGun6.UseVisualStyleBackColor = true;
            this.chkGun7.AutoSize = true;
            this.chkGun7.Checked = true;
            this.chkGun7.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkGun7.Location = new System.Drawing.Point(460, 61);
            this.chkGun7.Name = "chkGun7";
            this.chkGun7.Size = new System.Drawing.Size(45, 17);
            this.chkGun7.TabIndex = 9;
            this.chkGun7.Text = "Paz";
            this.chkGun7.UseVisualStyleBackColor = true;
            //
            // btnGorevKur
            //
            this.btnGorevKur.Location = new System.Drawing.Point(12, 92);
            this.btnGorevKur.Name = "btnGorevKur";
            this.btnGorevKur.Size = new System.Drawing.Size(170, 27);
            this.btnGorevKur.TabIndex = 10;
            this.btnGorevKur.Text = "Görevi Kur / Güncelle";
            this.btnGorevKur.UseVisualStyleBackColor = true;
            this.btnGorevKur.Click += new System.EventHandler(this.btnGorevKur_Click);
            //
            // btnGorevKaldir
            //
            this.btnGorevKaldir.Location = new System.Drawing.Point(190, 92);
            this.btnGorevKaldir.Name = "btnGorevKaldir";
            this.btnGorevKaldir.Size = new System.Drawing.Size(120, 27);
            this.btnGorevKaldir.TabIndex = 11;
            this.btnGorevKaldir.Text = "Görevi Kaldır";
            this.btnGorevKaldir.UseVisualStyleBackColor = true;
            this.btnGorevKaldir.Click += new System.EventHandler(this.btnGorevKaldir_Click);
            //
            // btnGorevSimdi
            //
            this.btnGorevSimdi.Location = new System.Drawing.Point(318, 92);
            this.btnGorevSimdi.Name = "btnGorevSimdi";
            this.btnGorevSimdi.Size = new System.Drawing.Size(170, 27);
            this.btnGorevSimdi.TabIndex = 12;
            this.btnGorevSimdi.Text = "Şimdi Çalıştır (test)";
            this.btnGorevSimdi.UseVisualStyleBackColor = true;
            this.btnGorevSimdi.Click += new System.EventHandler(this.btnGorevSimdi_Click);
            //
            // btnGorevYenile
            //
            this.btnGorevYenile.Location = new System.Drawing.Point(496, 92);
            this.btnGorevYenile.Name = "btnGorevYenile";
            this.btnGorevYenile.Size = new System.Drawing.Size(90, 27);
            this.btnGorevYenile.TabIndex = 13;
            this.btnGorevYenile.Text = "Yenile";
            this.btnGorevYenile.UseVisualStyleBackColor = true;
            this.btnGorevYenile.Click += new System.EventHandler(this.btnGorevYenile_Click);
            //
            // btnGorevZamanlayici
            //
            this.btnGorevZamanlayici.Location = new System.Drawing.Point(594, 92);
            this.btnGorevZamanlayici.Name = "btnGorevZamanlayici";
            this.btnGorevZamanlayici.Size = new System.Drawing.Size(200, 27);
            this.btnGorevZamanlayici.TabIndex = 14;
            this.btnGorevZamanlayici.Text = "Görev Zamanlayıcı\'yı Aç";
            this.btnGorevZamanlayici.UseVisualStyleBackColor = true;
            this.btnGorevZamanlayici.Click += new System.EventHandler(this.btnGorevZamanlayici_Click);
            //
            // lblGorevNot
            //
            this.lblGorevNot.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblGorevNot.ForeColor = System.Drawing.SystemColors.GrayText;
            this.lblGorevNot.Location = new System.Drawing.Point(12, 290);
            this.lblGorevNot.Name = "lblGorevNot";
            this.lblGorevNot.Size = new System.Drawing.Size(1100, 120);
            this.lblGorevNot.TabIndex = 2;
            //
            // statusStrip1
            //
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblStatus,
            this.lblHedef,
            this.lblMode});
            this.statusStrip1.Location = new System.Drawing.Point(0, 619);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1134, 22);
            this.statusStrip1.TabIndex = 1;
            //
            // lblStatus
            //
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(1000, 17);
            this.lblStatus.Spring = true;
            this.lblStatus.Text = "Hazır";
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // lblHedef
            //
            this.lblHedef.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Left;
            this.lblHedef.BorderStyle = System.Windows.Forms.Border3DStyle.Etched;
            this.lblHedef.Name = "lblHedef";
            this.lblHedef.Size = new System.Drawing.Size(60, 17);
            this.lblHedef.Text = "Hedef: -";
            //
            // lblMode
            //
            this.lblMode.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Left;
            this.lblMode.BorderStyle = System.Windows.Forms.Border3DStyle.Etched;
            this.lblMode.Name = "lblMode";
            this.lblMode.Size = new System.Drawing.Size(59, 17);
            this.lblMode.Text = "Mod: -";
            //
            // Form1
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1134, 641);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.statusStrip1);
            this.MinimumSize = new System.Drawing.Size(900, 560);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Üretim Doküman Senkronizasyonu";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.Shown += new System.EventHandler(this.Form1_Shown);
            this.tabControl1.ResumeLayout(false);
            this.tabIslemler.ResumeLayout(false);
            this.flowTop.ResumeLayout(false);
            this.flowTop.PerformLayout();
            this.panelBottom.ResumeLayout(false);
            this.tabGunluk.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.flowRunsTop.ResumeLayout(false);
            this.flowRunsTop.PerformLayout();
            this.flowLinesTop.ResumeLayout(false);
            this.flowLinesTop.PerformLayout();
            this.tabZamanlama.ResumeLayout(false);
            this.grpGorevDurum.ResumeLayout(false);
            this.grpGorevDurum.PerformLayout();
            this.grpGorevAyar.ResumeLayout(false);
            this.grpGorevAyar.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabIslemler;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ColumnHeader Proje;
        private System.Windows.Forms.ColumnHeader Klasör;
        private System.Windows.Forms.ColumnHeader Ürün_Kodu;
        private System.Windows.Forms.ColumnHeader Durum;
        private System.Windows.Forms.ColumnHeader Dosya_Durumu;
        private System.Windows.Forms.ColumnHeader BaslangicTarihi;
        private System.Windows.Forms.ColumnHeader PlanlananMiktar;
        private System.Windows.Forms.ColumnHeader UrunAdi;
        private System.Windows.Forms.ColumnHeader KaynakKodu;
        private System.Windows.Forms.FlowLayoutPanel flowTop;
        private System.Windows.Forms.Label lblTarih;
        private System.Windows.Forms.DateTimePicker dtpFrom;
        private System.Windows.Forms.Label lblTire;
        private System.Windows.Forms.DateTimePicker dtpTo;
        private System.Windows.Forms.Button btnListele;
        private System.Windows.Forms.Button btnKopyala;
        private System.Windows.Forms.Button btnBasla;
        private System.Windows.Forms.Button btnHedefAc;
        private System.Windows.Forms.Panel panelBottom;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.TabPage tabGunluk;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ListView lvRuns;
        private System.Windows.Forms.FlowLayoutPanel flowRunsTop;
        private System.Windows.Forms.Button btnGunlukYenile;
        private System.Windows.Forms.Button btnLogDosyasi;
        private System.Windows.Forms.Button btnLogKlasoru;
        private System.Windows.Forms.Label lblGunlukBilgi;
        private System.Windows.Forms.ListView lvLines;
        private System.Windows.Forms.FlowLayoutPanel flowLinesTop;
        private System.Windows.Forms.CheckBox chkSadeceSorun;
        private System.Windows.Forms.Label lblFiltre;
        private System.Windows.Forms.TextBox txtFiltre;
        private System.Windows.Forms.Label lblSatirBilgi;
        private System.Windows.Forms.TabPage tabZamanlama;
        private System.Windows.Forms.GroupBox grpGorevDurum;
        private System.Windows.Forms.Label lblGorevDurum;
        private System.Windows.Forms.Label lblGorevSonraki;
        private System.Windows.Forms.Label lblGorevSon;
        private System.Windows.Forms.Label lblGorevProgram;
        private System.Windows.Forms.GroupBox grpGorevAyar;
        private System.Windows.Forms.Label lblGorevSaat;
        private System.Windows.Forms.DateTimePicker dtpGorevSaat;
        private System.Windows.Forms.Label lblGorevGunler;
        private System.Windows.Forms.CheckBox chkGun1;
        private System.Windows.Forms.CheckBox chkGun2;
        private System.Windows.Forms.CheckBox chkGun3;
        private System.Windows.Forms.CheckBox chkGun4;
        private System.Windows.Forms.CheckBox chkGun5;
        private System.Windows.Forms.CheckBox chkGun6;
        private System.Windows.Forms.CheckBox chkGun7;
        private System.Windows.Forms.Button btnGorevKur;
        private System.Windows.Forms.Button btnGorevKaldir;
        private System.Windows.Forms.Button btnGorevSimdi;
        private System.Windows.Forms.Button btnGorevYenile;
        private System.Windows.Forms.Button btnGorevZamanlayici;
        private System.Windows.Forms.Label lblGorevNot;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel lblStatus;
        private System.Windows.Forms.ToolStripStatusLabel lblHedef;
        private System.Windows.Forms.ToolStripStatusLabel lblMode;
    }
}
