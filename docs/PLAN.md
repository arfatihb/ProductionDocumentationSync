# ProductionDocumentationSync — Plan

## Hedef
SAP B1 iş emirlerine göre PDM'den PDF'leri ve şablondan Excel kontrol formlarını üretim klasörüne üreten WinForms
aracının (a) yeni sunucuda yerel `D:\GebzeUretim` ile çalışması, (b) Görev Zamanlayıcı ile güvenilir otomatik
çalışması, (c) uygulama içinde anlaşılır bir çalışma günlüğü sunması, (d) Kopyala'nın sessizce hiçbir şey yapmaması
sorununun giderilmesi.

## Mevcut durum analizi (16 Ekim 2026)
- Tüm yollar koda gömülüydü (`\\192.168.100.66\GebzeUretim`, Antet.xlsx, _Raporlar, crash log). UNC erişilemeyince
  `Directory.CreateDirectory` her klasörde sessizce `continue` ediyor, kullanıcıya hiçbir mesaj gitmiyordu
  → "Kopyala çalışmıyor" algısı. Ayrıca MANUEL modda yakalanmayan hatalar yalnızca dosyaya yazılıyor, ekrana çıkmıyordu.
- `/auto` akışı `Form_Load` içinde `PerformClick` + `this.Close()` ile yapılıyordu: Load içinde Close güvenilir
  çalışmaz, `PerformClick` görünür/etkin kontrol şartına bağlıdır, uzun iş UI iş parçacığında çalışıp pencereyi
  donduruyordu; zaman aşımı/tek örnek koruması yoktu, çıkış kodu dönmüyordu.
- Günlük: her satırda tüm xlsx yeniden kaydediliyordu (UNC üzerinde çok yavaş), `Log` + `LogCsv` çift kayıt,
  ayrıca AutoRunLogs altına ikinci bir xlsx; okunması zor.
- csproj: kullanılmayan 7 DevExpress referansı; `..\packages` HintPath'leri .sln ile aynı klasörde olduğu için
  restore edilemiyordu; PDM interop yolu 6 seviye `..\..` ile göreli.
- `Başla` düğmesine hiç olay bağlı değildi.

## Kararlar

| Tarih | Karar | Gerekçe |
|---|---|---|
| 16.10.2026 | Tüm yollar + SAP bağlantısı + PDM kasa adı `App.config` (`appSettings`/`connectionStrings`) üzerinden; varsayılan `D:\GebzeUretim` | Sunucu değişince yeniden derleme gerekmesin |
| 16.10.2026 | Uzun işler arka planda STA iş parçacığında, UI `Invoke` ile güncellenir; PDM COM nesneleri bu iş parçacığında yaratılır, kasa bağlantısı tur başına bir kez | UI donmasın, canlı günlük görünsün |
| 16.10.2026 | `/auto`: `Shown` sonrası `BeginInvoke` ile başlar, bitince `Close`; tek örnek Mutex; `AutoTimeoutMinutes` bekçi zamanlayıcı; çıkış kodları 0/1/2/3/4/5 | Görev Zamanlayıcı'da asılı kalma ve çift çalışma önlensin, sonuç "Son Çalıştırma Sonucu"nda görünsün |
| 16.10.2026 | Zamanlanmış görev **Interactive** (yalnızca oturum açıkken) kurulur: `Tools\Register-AutoTask.ps1` | PDM istemcisi oturum 0'da çalışmaz |
| 16.10.2026 | Günlük: tek `RunLogger`; sekmeli `.log` anında, `.xlsx` oturum sonunda bir kez; AutoRunLogs klasörü kaldırıldı | Hız + tek kaynak |
| 16.10.2026 | Uygulama içi "Çalışma Günlüğü" sekmesi: geçmiş çalışmalar (Run_*.log'dan özet) + satır ayrıntısı + hata filtresi + arama; mevcut oturum canlı | Excel'e bakmadan "ne zaman çalıştı, ne yaptı" görülsün |
| 16.10.2026 | NuGet `PackageReference`'a geçildi, DevExpress referansları silindi, PDM interop yolu `lib\` → Program Files (2 varyant) sırasıyla aranır | Temiz makinede derlensin |
| 16.10.2026 | `Başla` düğmesi "Listele + Kopyala" olarak bağlandı; Kopyala listesiz basılırsa uyarır | Eksik davranış |
| 16.10.2026 | Operasyonu (Kaynak Kodu) olmayan ürün "Operasyon yok" olarak işaretlenir (eskiden "Kopyalandı" görünüyordu) | Yanıltıcı durum |
| 16.10.2026 | Zamanlama uygulama içinden: "Zamanlama" sekmesi, Schedule.Service COM API (geç bağlama, ek paket yok) ile görev kur/güncelle/kaldır/şimdi çalıştır + durum; saat ve günler seçilebilir | Kullanıcı saati script'siz, Windows görevi gibi ayarlamak istedi |
| 16.10.2026 | Otomatik çalışma görünürlüğü: ilerleme durum çubuğunda (her sekmede) + pencere başlığında + görev çubuğu düğmesinde (ITaskbarList3) + tepsi simgesi/balon; bitince özet `AutoCloseDelaySeconds` (10 sn) sonra kapanır; ikinci örnek açılırsa çalışan pencere öne gelir (WM_APP mesajı); görev pencere açıkken tetiklenirse günlükte SKIP + açık pencerede uyarı | Kullanıcı arka planda çalıştığını göremiyor, yeniden açınca ne olacağını bilmiyordu |
| 16.10.2026 | "İşlenecek günler" (AutoLookbackDays) Zamanlama sekmesinden ayarlanır, exe.config'e yazılır; "Günlükleri Temizle…" N günden eski Run_/Crash_ dosyalarını siler | Kullanıcı isteği |

## Vazgeçilenler
- PDM interop için stub/taklit ile derleme doğrulaması — vazgeçildi: kullanıcı gerçek ortam kurdu (VS 2022 + PDM istemcisi).
- UI iş parçacığında `Application.DoEvents` ile ilerleme — vazgeçildi: yeniden giriş riski, arka plan iş parçacığı tercih edildi.

## Fazlar
1. ✅ Analiz ve kararlar
2. ✅ Kod: AppConfig, RunLogger, RunHistory, Form1 (worker + günlük sekmesi), Program, Designer, csproj, App.config
3. ✅ Derleme doğrulaması (Release, 0 hata/0 uyarı) + /auto duman testi + manuel UI görsel kontrol (bu makinede)
4. ⬜ Yeni sunucuda test: elle Listele/Kopyala, `Run-Auto-Test.cmd`, görev kaydı, ertesi gün otomatik çalışma kontrolü
5. ⬜ Geri bildirime göre düzeltmeler
