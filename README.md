# Üretim Doküman Senkronizasyonu (ProductionDocumentationSync)

SAP Business One'daki iş emirlerini okur, her ürün için SOLIDWORKS PDM kasasından PDF'i bulup
`TargetRoot\Proje\Klasör\Operasyon\` altına kopyalar ve şablondan (Antet.xlsx) kontrol formu Excel'leri üretir.
Elle (pencereden) ya da Görev Zamanlayıcı ile her gün otomatik (`/auto`) çalışır.

Plan ve güncel durum: [docs/PLAN.md](docs/PLAN.md), [docs/STATUS.md](docs/STATUS.md).

## Derleme

- Visual Studio 2022+ (".NET desktop development" iş yükü, .NET Framework 4.8).
- SOLIDWORKS PDM istemcisi kurulu olmalı **veya** `EPDM.Interop.epdm.dll` proje içindeki `lib\` klasörüne konmalı
  (csproj sırayla `lib\`, `C:\Program Files\SOLIDWORKS Corp\SOLIDWORKS PDM\`, `...PDM (2)\` altında arar).
- NuGet paketleri PackageReference ile otomatik iner. DevExpress gerekmez.
- Release derlemesi: `bin\Release\ProductionDocumentationSync.exe` (Costura ile bağımlılıklar exe içine gömülüdür)
  + `ProductionDocumentationSync.exe.config`.

```powershell
& "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" ProductionDocumentationSync.csproj -restore -p:Configuration=Release
```

## Sunucuya kurulum

1. `bin\Release\ProductionDocumentationSync.exe`, `ProductionDocumentationSync.exe.config` ve `Tools\` klasörünü
   sunucuda bir klasöre kopyalayın (örn. `D:\Apps\ProductionDocumentationSync\`).
2. `D:\GebzeUretim` klasörünü oluşturun ve Excel şablonunu `D:\GebzeUretim\Antet.xlsx` olarak koyun.
3. `ProductionDocumentationSync.exe.config` içindeki ayarları kontrol edin (aşağıda).
4. Sunucuda SOLIDWORKS PDM istemcisi kurulu ve görevin çalışacağı Windows kullanıcısı için `BERRMAK` kasa görünümü
   oluşturulmuş + **otomatik oturum açma** ayarlanmış olmalı (aksi halde `/auto` modunda PDM giriş penceresi açılır ve bekler).
5. SQL sunucusuna (`connectionStrings/Sap`) erişim olmalı.
6. Elle test: exe'yi açın → **Listele** → **Kopyala** (ya da **Listele + Kopyala**). Sonuçlar ve hatalar
   **Çalışma Günlüğü** sekmesinde.
7. Otomatik test: `Tools\Run-Auto-Test.cmd` → pencere açılır, çalışır, kendini kapatır; çıkış kodu gösterilir.
8. Görevi kurun: uygulamada **Zamanlama** sekmesi → saat ve günleri seçin → **Görevi Kur / Güncelle**.
   Aynı sekmede görevin durumu, sonraki/son çalışma ve son sonuç görünür; **Şimdi Çalıştır (test)** görevi hemen
   tetikler (pencere kapanır, otomatik çalışma ayrı açılıp biter). Komut satırı alternatifi:

```powershell
powershell -ExecutionPolicy Bypass -File .\Tools\Register-AutoTask.ps1 -ExePath "D:\Apps\ProductionDocumentationSync\ProductionDocumentationSync.exe" -Time 07:00
```

> Görev **"yalnızca kullanıcı oturum açtığında"** çalışacak şekilde kurulur; PDM istemcisi etkileşimsiz oturumda
> çalışmaz. Sunucuda bu kullanıcının oturumu açık kalmalıdır (ekran kilitlenebilir). Görev saatinde uygulama
> penceresi açıksa görev "zaten çalışıyor" (4) ile atlanır; işiniz bitince pencereyi kapatın.

## Ayarlar (`ProductionDocumentationSync.exe.config`)

| Anahtar | Varsayılan | Açıklama |
|---|---|---|
| `TargetRoot` | `D:\GebzeUretim` | PDF/Excel çıktılarının kök klasörü |
| `TemplateXlsx` | `TargetRoot\Antet.xlsx` | Excel şablonu |
| `LogDir` | `TargetRoot\_Raporlar` | Günlükler (`Run_*.log`, `Run_*.xlsx`, `Crash_*.log`) |
| `PdmVault` | `BERRMAK` | PDM kasa adı |
| `AutoLookbackDays` | `0` | `/auto` modunda bugünden kaç gün geriye bakılsın |
| `AutoTimeoutMinutes` | `120` | `/auto` modunda azami süre; aşılırsa çıkış kodu 3 |
| `connectionStrings/Sap` | – | SAP veritabanı bağlantısı |

## Çıkış kodları (`/auto`)

| Kod | Anlamı |
|---|---|
| 0 | Başarılı |
| 1 | Çökme (ayrıntı `Run_*.log` içinde CRASH satırı ve `Crash_*.log`) |
| 2 | Tamamlandı ama kopyalanamayan PDF var |
| 3 | Zaman aşımı (`AutoTimeoutMinutes`) |
| 4 | Zaten çalışan bir örnek var |
| 5 | Yapılandırma hatası (hedef klasör / bağlantı dizesi) |

Görev Zamanlayıcı'da "Son Çalıştırma Sonucu" sütunu bu kodu gösterir (0x0, 0x2 ...).

## Günlükler

Her çalışma `LogDir\Run_<tarih>_<kullanıcı>.log` (sekmeyle ayrılmış, anında yazılır) ve bitişte aynı içerikle
`Run_*.xlsx` üretir. Uygulamadaki **Çalışma Günlüğü** sekmesi bu dosyaları okuyup çalışma listesi
(başlangıç, mod, süre, sonuç, PDF OK/hata, Excel) ve satır ayrıntısı olarak gösterir; "Sadece hata / uyarı" filtresi
ve arama vardır. Satıra çift tıklayınca ilgili dosya/klasör Explorer'da açılır.
