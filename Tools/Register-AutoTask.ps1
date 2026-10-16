<#
.SYNOPSIS
  ProductionDocumentationSync'i her gün belirli saatte "/auto" ile çalıştıran zamanlanmış görevi kurar.

.DESCRIPTION
  Görev, "yalnızca kullanıcı oturum açtığında" (Interactive) çalışacak şekilde kaydedilir. Bu ZORUNLUDUR:
  SOLIDWORKS PDM istemcisi (COM) oturum 0'da / etkileşimsiz oturumda çalışmaz; "Kullanıcı oturum açmış olsa da
  olmasa da çalıştır" seçilirse PDM'e bağlanılamaz, uygulama ya hata verir ya da askıda kalır.
  Dolayısıyla sunucuda görevin sahibi Windows kullanıcısı sürekli oturum açık tutulmalıdır (ekran kilitli olabilir).

.PARAMETER ExePath
  ProductionDocumentationSync.exe'nin tam yolu. Verilmezse önce scriptin yanında, sonra ..\bin\Release altında aranır.
.PARAMETER Time
  Günlük çalışma saati (HH:mm). Varsayılan 07:00.
.PARAMETER TaskName
  Görev adı. Varsayılan ProductionDocumentationSync.
.PARAMETER TimeLimitHours
  Görev bu süreyi aşarsa Görev Zamanlayıcı tarafından durdurulur. Varsayılan 3 saat
  (uygulamanın kendi AutoTimeoutMinutes ayarı daha kısadır ve önce devreye girer).
.PARAMETER Remove
  Görevi siler.

.EXAMPLE
  powershell -ExecutionPolicy Bypass -File .\Register-AutoTask.ps1 -ExePath "D:\Apps\ProductionDocumentationSync\ProductionDocumentationSync.exe" -Time 07:00
.EXAMPLE
  powershell -ExecutionPolicy Bypass -File .\Register-AutoTask.ps1 -Remove
#>
param(
    [string]$ExePath,
    [string]$Time = "07:00",
    [string]$TaskName = "ProductionDocumentationSync",
    [int]$TimeLimitHours = 3,
    [switch]$Remove
)

$ErrorActionPreference = "Stop"

if ($Remove) {
    $existing = Get-ScheduledTask -TaskName $TaskName -ErrorAction SilentlyContinue
    if ($null -eq $existing) { Write-Host "Görev zaten yok: $TaskName"; return }
    Unregister-ScheduledTask -TaskName $TaskName -Confirm:$false
    Write-Host "Görev silindi: $TaskName"
    return
}

if ([string]::IsNullOrWhiteSpace($ExePath)) {
    $candidates = @(
        (Join-Path $PSScriptRoot "ProductionDocumentationSync.exe"),
        (Join-Path $PSScriptRoot "..\ProductionDocumentationSync.exe"),
        (Join-Path $PSScriptRoot "..\bin\Release\ProductionDocumentationSync.exe")
    )
    foreach ($c in $candidates) { if (Test-Path $c) { $ExePath = $c; break } }
    if ([string]::IsNullOrWhiteSpace($ExePath)) { throw "ProductionDocumentationSync.exe bulunamadı; -ExePath verin." }
}
$ExePath = (Resolve-Path $ExePath).Path
$workDir = Split-Path $ExePath -Parent
if (-not (Test-Path (Join-Path $workDir "ProductionDocumentationSync.exe.config"))) {
    Write-Warning "exe'nin yanında ProductionDocumentationSync.exe.config yok; ayarlar varsayılan değerlerle çalışır."
}

$parsed = [DateTime]::ParseExact($Time, "HH:mm", $null)

$action    = New-ScheduledTaskAction -Execute $ExePath -Argument "/auto" -WorkingDirectory $workDir
$trigger   = New-ScheduledTaskTrigger -Daily -At $parsed
$settings  = New-ScheduledTaskSettingsSet `
                -ExecutionTimeLimit (New-TimeSpan -Hours $TimeLimitHours) `
                -MultipleInstances IgnoreNew `
                -StartWhenAvailable `
                -AllowStartIfOnBatteries -DontStopIfGoingOnBatteries
$user      = "$env:USERDOMAIN\$env:USERNAME"
# Interactive = "Yalnızca kullanıcı oturum açtığında çalıştır" (PDM COM için zorunlu)
$principal = New-ScheduledTaskPrincipal -UserId $user -LogonType Interactive -RunLevel Limited

Register-ScheduledTask -TaskName $TaskName -Action $action -Trigger $trigger -Settings $settings -Principal $principal `
    -Description "SAP iş emirlerine göre PDM'den PDF ve Excel üretir (ProductionDocumentationSync /auto)." -Force | Out-Null

Write-Host ""
Write-Host "Görev kaydedildi : $TaskName"
Write-Host "Program          : $ExePath /auto"
Write-Host "Başlangıç klasörü: $workDir"
Write-Host "Zamanlama        : Her gün $Time, kullanıcı: $user (yalnızca oturum açıkken)"
Write-Host ""
Write-Host "Hemen denemek için :  Start-ScheduledTask -TaskName $TaskName"
Write-Host "Son sonucu görmek  :  (Get-ScheduledTaskInfo -TaskName $TaskName) | Select LastRunTime, LastTaskResult"
Write-Host "Çıkış kodları      :  0=OK  1=çökme  2=kopyalama hataları  3=zaman aşımı  4=zaten çalışıyor  5=yapılandırma hatası"
