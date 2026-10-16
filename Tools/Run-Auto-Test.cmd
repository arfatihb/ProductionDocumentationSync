@echo off
rem Uygulamayi zamanlanmis gorevin yaptigi gibi "/auto" ile calistirir ve cikis kodunu gosterir.
rem Gorevi kurmadan once bununla test edin.
setlocal
set "EXE=%~dp0ProductionDocumentationSync.exe"
if not exist "%EXE%" set "EXE=%~dp0..\ProductionDocumentationSync.exe"
if not exist "%EXE%" set "EXE=%~dp0..\bin\Release\ProductionDocumentationSync.exe"
if not exist "%EXE%" (
    echo ProductionDocumentationSync.exe bulunamadi.
    pause
    exit /b 9
)
echo Calistiriliyor: "%EXE%" /auto
start "" /wait "%EXE%" /auto
set RC=%ERRORLEVEL%
echo.
echo Cikis kodu: %RC%
echo   0 = basarili   1 = cokme   2 = kopyalama hatalari var   3 = zaman asimi
echo   4 = zaten calisiyor   5 = yapilandirma hatasi
echo Ayrintilar: uygulamadaki "Calisma Gunlugu" sekmesi ya da _Raporlar\Run_*.log
pause
exit /b %RC%
