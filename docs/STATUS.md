# ProductionDocumentationSync — Durum

Son güncelleme: 16 Ekim 2026 — Manuel mod sunucuda doğrulandı; uygulama içi "Zamanlama" sekmesi (Görev Zamanlayıcı kur/güncelle/kaldır/şimdi çalıştır) eklendi ve gerçek görev testi geçti. Push kimlik sorunu nedeniyle bekliyor.

## Şu an nerede kaldık
Kullanıcı yeni sunucuda manuel modu (Listele/Kopyala) sorunsuz çalıştırdı. İstek üzerine zamanlama uygulama içine
alındı: "Zamanlama" sekmesinde saat + günler seçilip görev kuruluyor; durum, sonraki/son çalışma ve sonuç görünüyor.
`ScheduledTaskManager` (Schedule.Service COM, geç bağlama) bu makinede ayrı bir test görev adıyla gerçek Görev
Zamanlayıcı'ya karşı test edildi: haftalık kayıt, günlük güncelleme, şimdi çalıştır (sonuç 0), kaldırma → hepsi geçti.
Release derlemesi 0 hata / 0 uyarı. Git: yerelde 2 commit ileride; push için `arfatihb` yetkili kimlik gerekiyor
(makinedeki kayıtlı kimlik `fbgtadv`).

## Yarım kalan
- Push (kimlik). Kod tarafında yarım iş yok.

## Sıradaki adımlar
1. Push (VS Code Source Control ile `arfatihb` hesabı ya da Windows Kimlik Bilgileri'ndeki github kaydını güncelle).
2. Sunucuda yeni exe'yi kopyala → Zamanlama sekmesi → saat/gün seç → "Görevi Kur" → "Şimdi Çalıştır (test)" ile dene.
3. Ertesi gün Zamanlama sekmesinde "Son çalışma / Sonuç" ve Çalışma Günlüğü'nde AUTO satırını kontrol et.
4. Sorun çıkarsa `_Raporlar\Run_*.log` dosyasını paylaş.

## Bilinen notlar
- Uygulama 32-bit tercihli (anycpu32bitpreferred, eski derlemeyle aynı). PDM istemcisi 32-bit COM'u da kaydettiği için sorun beklenmiyor.
- Günlük klasörü yazılamazsa (sürücü yok / erişim yok / çok uzun yol) otomatik olarak
  `%LOCALAPPDATA%\ProductionDocumentationSync\Logs` kullanılır ve günlükte WARN CONFIG satırı düşer.
- Git Bash `/auto` argümanını yola çevirir; test için cmd/PowerShell ya da `Run-Auto-Test.cmd` kullan.

## Tamamlananlar (yeniden eskiye)
- 16.10.2026 — "Zamanlama" sekmesi + ScheduledTaskManager (Görev Zamanlayıcı COM); gerçek görevle test; Program.ReleaseSingleInstance (Şimdi Çalıştır için).
- 16.10.2026 — Kullanıcı sunucuda manuel modu doğruladı; VS'den paket sürümleri güncellendi (ClosedXML 0.105.1, Costura 6.2.0, Fody 6.9.3).
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
