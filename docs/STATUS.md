# ProductionDocumentationSync — Durum

Son güncelleme: 16 Ekim 2026 — Yerel hedef (D:\GebzeUretim), /auto düzeltmesi, günlük sekmesi ve csproj temizliği derlendi ve duman testi geçti; sırada yeni sunucuda gerçek test.

## Şu an nerede kaldık
Kod değişiklikleri tamamlandı, Release derlemesi bu makinede (VS 18 Community MSBuild) 0 hata / 0 uyarı ile alındı.
Duman testi: `/auto` ile başlatıldığında uygulama kendiliğinden çalışıp SQL hatasını günlükledi, Run_*.log + Run_*.xlsx
yazdı, kendini kapattı ve çıkış kodu 1 döndürdü (SQL sunucusuna bu laptoptan erişim yok; beklenen). Manuel modda
iki sekme de düzgün açılıyor. PDM araması/kopyalama ve şablondan Excel üretimi bu makinede test EDİLEMEDİ
(kasa ve SQL yok) — yeni sunucuda doğrulanacak.

## Yarım kalan
- Yok (kod tarafında). Sunucu testi bekleniyor.

## Sıradaki adımlar
1. Yeni sunucuda kurulum (README.md "Sunucuya kurulum"): exe + exe.config + Tools\; `D:\GebzeUretim\Antet.xlsx`;
   exe.config'de `Sap` bağlantısı ve `PdmVault` kontrolü.
2. Elle test: Listele → Kopyala; Çalışma Günlüğü sekmesinde PDM/EXCEL satırlarını kontrol et.
   Özellikle PDM'in arka plan iş parçacığından çalıştığını doğrula (günlükte `PDM Kasaya bağlanıldı` ve `COPY OK`).
3. `Tools\Run-Auto-Test.cmd` → çıkış kodu 0 veya 2 beklenir.
4. `Tools\Register-AutoTask.ps1 -ExePath ... -Time 07:00`; ertesi gün Çalışma Günlüğü sekmesinde AUTO satırı.
5. Sorun çıkarsa `_Raporlar\Run_*.log` dosyasını paylaş; geri bildirime göre düzeltme.

## Bilinen notlar
- Uygulama 32-bit tercihli (anycpu32bitpreferred, eski derlemeyle aynı). PDM istemcisi 32-bit COM'u da kaydettiği için sorun beklenmiyor.
- Günlük klasörü yazılamazsa (sürücü yok / erişim yok / çok uzun yol) otomatik olarak
  `%LOCALAPPDATA%\ProductionDocumentationSync\Logs` kullanılır ve günlükte WARN CONFIG satırı düşer.
- Git Bash `/auto` argümanını yola çevirir; test için cmd/PowerShell ya da `Run-Auto-Test.cmd` kullan.

## Tamamlananlar (yeniden eskiye)
- 16.10.2026 — Release derlemesi + /auto duman testi + manuel UI görsel kontrolü (bu makinede).
- 16.10.2026 — RunLogger: dosya düzeyinde de yedek klasöre düşme; CRASH mesajında aşama (Listeleme/Kopyalama).
- 16.10.2026 — App.config'e TargetRoot/TemplateXlsx/LogDir/PdmVault/AutoLookbackDays/AutoTimeoutMinutes + Sap bağlantısı; AppConfig.Validate başlangıç kontrolü.
- 16.10.2026 — RunLogger (tek günlük, .log anında, .xlsx sonda) ve RunHistory (Run_*.log özetleme).
- 16.10.2026 — Form1: arka plan STA iş parçacığı, canlı durum çubuğu, Çalışma Günlüğü sekmesi (çalışma listesi + satırlar + filtre/arama + Explorer'da aç), Başla = Listele + Kopyala, Kopyala sonunda özet mesaj.
- 16.10.2026 — /auto: Shown sonrası başlatma, tek örnek Mutex, zaman aşımı bekçisi, çıkış kodları; Tools\Register-AutoTask.ps1 ve Run-Auto-Test.cmd.
- 16.10.2026 — csproj: PackageReference, DevExpress kaldırıldı, PDM interop yolu otomatik; packages.config silindi.

## Yerelde çalıştırma
- Derleme: VS 2022+ (".NET desktop development") + SOLIDWORKS PDM istemcisi (veya `lib\EPDM.Interop.epdm.dll`).
  `& "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" ProductionDocumentationSync.csproj -restore -p:Configuration=Release`
- Çalıştırma: `bin\Release\ProductionDocumentationSync.exe` (elle) / `... /auto` (otomatik). Ayarlar `ProductionDocumentationSync.exe.config`.
- Gerekenler: `D:\GebzeUretim\Antet.xlsx` şablonu, SAP SQL erişimi (bağlantı dizesi App.config'de), PDM kasa görünümü (BERRMAK) oturum açmış.
- D: sürücüsü olmayan makinede elle denemek için exe.config'de TargetRoot geçici olarak kısa bir yerel yol yapılabilir (commit etme).
