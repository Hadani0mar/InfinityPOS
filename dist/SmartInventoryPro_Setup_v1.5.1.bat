@echo off
chcp 65001 >nul
echo ========================================
echo    SmartInventory Pro Installer v1.5.1
echo ========================================
echo.

set "APP_NAME=SmartInventory Pro"
set "APP_DIR=%PROGRAMFILES%\%APP_NAME%"
set "START_MENU=%APPDATA%\Microsoft\Windows\Start Menu\Programs"

echo [INFO] تثبيت %APP_NAME%...
echo.

REM Create application directory
if not exist "%APP_DIR%" (
    mkdir "%APP_DIR%"
    echo [SUCCESS] تم إنشاء مجلد التطبيق
)

REM Copy application files
echo [INFO] نسخ ملفات التطبيق...
if exist "..\bin\Release\net8.0-windows\win-x64\publish\*" (
    xcopy "..\bin\Release\net8.0-windows\win-x64\publish\*" "%APP_DIR%\" /Y /E /I
) else (
    echo [ERROR] ملفات التطبيق غير موجودة. تأكد من بناء التطبيق أولاً.
    pause
    exit /b 1
)

if exist "..\appsettings.json" copy "..\appsettings.json" "%APP_DIR%\" >nul
if exist "..\DOWNLOAD_FROM_GITHUB.md" copy "..\DOWNLOAD_FROM_GITHUB.md" "%APP_DIR%\" >nul

echo [SUCCESS] تم نسخ ملفات التطبيق

REM Create Start Menu shortcut
echo [INFO] إنشاء اختصار في قائمة ابدأ...
powershell -Command "$WshShell = New-Object -comObject WScript.Shell; $Shortcut = $WshShell.CreateShortcut('%START_MENU%\%APP_NAME%.lnk'); $Shortcut.TargetPath = '%APP_DIR%\SmartInventoryPro.exe'; $Shortcut.WorkingDirectory = '%APP_DIR%'; $Shortcut.Description = 'SmartInventory Pro - Advanced Business Management System'; $Shortcut.Save()"

REM Create Desktop shortcut
echo [INFO] إنشاء اختصار على سطح المكتب...
set "DESKTOP_SHORTCUT=%USERPROFILE%\Desktop\%APP_NAME%.lnk"
powershell -Command "$WshShell = New-Object -comObject WScript.Shell; $Shortcut = $WshShell.CreateShortcut('%DESKTOP_SHORTCUT%'); $Shortcut.TargetPath = '%APP_DIR%\SmartInventoryPro.exe'; $Shortcut.WorkingDirectory = '%APP_DIR%'; $Shortcut.Description = 'SmartInventory Pro - Advanced Business Management System'; $Shortcut.Save()"

REM Create uninstaller
echo [INFO] إنشاء أداة إلغاء التثبيت...
echo @echo off > "%APP_DIR%\Uninstall.bat"
echo chcp 65001 ^>nul >> "%APP_DIR%\Uninstall.bat"
echo echo إلغاء تثبيت %APP_NAME%... >> "%APP_DIR%\Uninstall.bat"
echo taskkill /f /im SmartInventoryPro.exe 2^>nul >> "%APP_DIR%\Uninstall.bat"
echo timeout /t 2 /nobreak ^>nul >> "%APP_DIR%\Uninstall.bat"
echo rmdir /s /q "%APP_DIR%" >> "%APP_DIR%\Uninstall.bat"
echo del "%START_MENU%\%APP_NAME%.lnk" >> "%APP_DIR%\Uninstall.bat"
echo del "%DESKTOP_SHORTCUT%" >> "%APP_DIR%\Uninstall.bat"
echo echo تم إلغاء التثبيت بنجاح! >> "%APP_DIR%\Uninstall.bat"
echo pause >> "%APP_DIR%\Uninstall.bat"

echo.
echo ========================================
echo    تم التثبيت بنجاح!
echo ========================================
echo.
echo تم تثبيت %APP_NAME% في: %APP_DIR%
echo تم إنشاء اختصارات في قائمة ابدأ وسطح المكتب
echo.
echo للبدء: اضغط على الاختصار أو اذهب إلى %APP_DIR%
echo لإلغاء التثبيت: شغل %APP_DIR%\Uninstall.bat
echo.
pause
