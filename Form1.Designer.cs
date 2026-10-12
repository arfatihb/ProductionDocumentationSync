namespace ProductionDocumentationSync
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.ProgressBar progressBar1;
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

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
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
            this.btnBasla = new System.Windows.Forms.Button();
            this.btnKopyala = new System.Windows.Forms.Button();
            this.btnListele = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // progressBar1
            // 
            this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar1.Location = new System.Drawing.Point(0, 326);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(943, 23);
            this.progressBar1.TabIndex = 4;
            // 
            // listView1
            // 
            this.listView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
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
            this.listView1.FullRowSelect = true;
            this.listView1.GridLines = true;
            this.listView1.HideSelection = false;
            this.listView1.Location = new System.Drawing.Point(15, 48);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(943, 228);
            this.listView1.TabIndex = 3;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            // 
            // Proje
            // 
            this.Proje.Text = "Proje";
            // 
            // Klasör
            // 
            this.Klasör.Text = "Klasör";
            // 
            // Ürün_Kodu
            // 
            this.Ürün_Kodu.Text = "Ürün Kodu";
            // 
            // Durum
            // 
            this.Durum.Text = "Durum";
            // 
            // Dosya_Durumu
            // 
            this.Dosya_Durumu.Text = "Dosya Durumu";
            // 
            // BaslangicTarihi
            // 
            this.BaslangicTarihi.Text = "Başlangıç";
            // 
            // PlanlananMiktar
            // 
            this.PlanlananMiktar.Text = "Miktar";
            // 
            // UrunAdi
            // 
            this.UrunAdi.Text = "Ürün Adı";
            // 
            // KaynakKodu
            // 
            this.KaynakKodu.Text = "Kaynak Kodu";
            // 
            // btnBasla
            // 
            this.btnBasla.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnBasla.Location = new System.Drawing.Point(177, 7);
            this.btnBasla.Name = "btnBasla";
            this.btnBasla.Size = new System.Drawing.Size(75, 23);
            this.btnBasla.TabIndex = 5;
            this.btnBasla.Text = "Başla";
            this.btnBasla.UseVisualStyleBackColor = true;
            // 
            // btnKopyala
            // 
            this.btnKopyala.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnKopyala.Location = new System.Drawing.Point(96, 7);
            this.btnKopyala.Name = "btnKopyala";
            this.btnKopyala.Size = new System.Drawing.Size(75, 23);
            this.btnKopyala.TabIndex = 0;
            this.btnKopyala.Text = "Kopyala";
            this.btnKopyala.UseVisualStyleBackColor = true;
            this.btnKopyala.Click += new System.EventHandler(this.btnKopyala_Click);
            // 
            // btnListele
            // 
            this.btnListele.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnListele.Location = new System.Drawing.Point(15, 7);
            this.btnListele.Name = "btnListele";
            this.btnListele.Size = new System.Drawing.Size(75, 23);
            this.btnListele.TabIndex = 1;
            this.btnListele.Text = "Listele";
            this.btnListele.UseVisualStyleBackColor = true;
            this.btnListele.Click += new System.EventHandler(this.btnListele_Click);
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.Controls.Add(this.btnBasla);
            this.panel1.Controls.Add(this.btnListele);
            this.panel1.Controls.Add(this.btnKopyala);
            this.panel1.Location = new System.Drawing.Point(668, 282);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(275, 33);
            this.panel1.TabIndex = 6;
            // 
            // Form1
            // 
            this.ClientSize = new System.Drawing.Size(944, 351);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.listView1);
            this.Controls.Add(this.progressBar1);
            this.Name = "Form1";
            this.Text = "Üretim Doküman Senkronizasyonu";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.Resize += new System.EventHandler(this.Form1_Resize);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        private System.Windows.Forms.Button btnBasla;
        private System.Windows.Forms.Button btnKopyala;
        private System.Windows.Forms.Button btnListele;
        private System.Windows.Forms.Panel panel1;
    }
}
