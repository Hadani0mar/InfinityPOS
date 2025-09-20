@echo off
echo ========================================
echo    SmartInventory Pro - Installer
echo ========================================
echo.

:: التحقق من صلاحيات المدير
net session >nul 2>&1
if %errorLevel% == 0 (
    echo [INFO] تم التحقق من صلاحيات المدير
) else (
    echo [ERROR] يجب تشغيل هذا الملف كمدير (Run as Administrator)
    echo اضغط على زر الفأرة الأيمن واختر "Run as administrator"
    pause
    exit /b 1
)

:: إنشاء مجلد التطبيق
set "APP_DIR=C:\Program Files\SmartInventoryPro"
echo [INFO] إنشاء مجلد التطبيق: %APP_DIR%
if not exist "%APP_DIR%" mkdir "%APP_DIR%"

:: نسخ الملفات
echo [INFO] نسخ ملفات التطبيق...
copy "SmartInventoryPro.exe" "%APP_DIR%\" >nul
if %errorLevel% == 0 (
    echo [SUCCESS] تم نسخ SmartInventoryPro.exe بنجاح
) else (
    echo [ERROR] فشل في نسخ SmartInventoryPro.exe
    pause
    exit /b 1
)

:: نسخ ملف الإعدادات
if exist "appsettings.json" (
    copy "appsettings.json" "%APP_DIR%\" >nul
    echo [SUCCESS] تم نسخ ملف الإعدادات
) else (
    echo [WARNING] ملف appsettings.json غير موجود، سيتم إنشاء ملف افتراضي
    echo {} > "%APP_DIR%\appsettings.json"
)

:: إنشاء اختصار في قائمة ابدأ
echo [INFO] إنشاء اختصار في قائمة ابدأ...
set "START_MENU=%APPDATA%\Microsoft\Windows\Start Menu\Programs"
set "SHORTCUT_PATH=%START_MENU%\SmartInventory Pro.lnk"

:: استخدام PowerShell لإنشاء الاختصار مع الأيقونة
powershell -Command "$WshShell = New-Object -comObject WScript.Shell; $Shortcut = $WshShell.CreateShortcut('%SHORTCUT_PATH%'); $Shortcut.TargetPath = '%APP_DIR%\SmartInventoryPro.exe'; $Shortcut.WorkingDirectory = '%APP_DIR%'; $Shortcut.Description = 'SmartInventory Pro - Advanced Business Management System'; $Shortcut.IconLocation = '%APP_DIR%\SmartInventoryPro.exe,0'; $Shortcut.Save()"

if exist "%SHORTCUT_PATH%" (
    echo [SUCCESS] تم إنشاء اختصار في قائمة ابدأ
) else (
    echo [WARNING] فشل في إنشاء اختصار في قائمة ابدأ
)

:: إنشاء اختصار على سطح المكتب
echo [INFO] إنشاء اختصار على سطح المكتب...
set "DESKTOP_SHORTCUT=%USERPROFILE%\Desktop\SmartInventory Pro.lnk"
powershell -Command "$WshShell = New-Object -comObject WScript.Shell; $Shortcut = $WshShell.CreateShortcut('%DESKTOP_SHORTCUT%'); $Shortcut.TargetPath = '%APP_DIR%\SmartInventoryPro.exe'; $Shortcut.WorkingDirectory = '%APP_DIR%'; $Shortcut.Description = 'SmartInventory Pro - Advanced Business Management System'; $Shortcut.IconLocation = '%APP_DIR%\SmartInventoryPro.exe,0'; $Shortcut.Save()"

if exist "%DESKTOP_SHORTCUT%" (
    echo [SUCCESS] تم إنشاء اختصار على سطح المكتب
) else (
    echo [WARNING] فشل في إنشاء اختصار على سطح المكتب
)

:: إنشاء ملف إلغاء التثبيت
echo [INFO] إنشاء ملف إلغاء التثبيت...
echo @echo off > "%APP_DIR%\Uninstall.bat"
echo echo ======================================== >> "%APP_DIR%\Uninstall.bat"
echo echo    SmartInventory Pro - Uninstaller >> "%APP_DIR%\Uninstall.bat"
echo echo ======================================== >> "%APP_DIR%\Uninstall.bat"
echo echo. >> "%APP_DIR%\Uninstall.bat"
echo echo [INFO] جاري إلغاء تثبيت SmartInventory Pro... >> "%APP_DIR%\Uninstall.bat"
echo echo. >> "%APP_DIR%\Uninstall.bat"
echo del "%SHORTCUT_PATH%" 2^>nul >> "%APP_DIR%\Uninstall.bat"
echo del "%DESKTOP_SHORTCUT%" 2^>nul >> "%APP_DIR%\Uninstall.bat"
echo rmdir /s /q "%APP_DIR%" 2^>nul >> "%APP_DIR%\Uninstall.bat"
echo echo [SUCCESS] تم إلغاء تثبيت SmartInventory Pro بنجاح >> "%APP_DIR%\Uninstall.bat"
echo pause >> "%APP_DIR%\Uninstall.bat"

echo.
echo ========================================
echo    تم تثبيت SmartInventory Pro بنجاح!
echo ========================================
echo.
echo [INFO] تم تثبيت التطبيق في: %APP_DIR%
echo [INFO] تم إنشاء اختصار في قائمة ابدأ
echo [INFO] تم إنشاء اختصار على سطح المكتب
echo [INFO] يمكنك إلغاء التثبيت من: %APP_DIR%\Uninstall.bat
echo.
echo [INFO] الميزات الجديدة:
echo - نظام التحديثات التلقائي من GitHub
echo - حفظ إعدادات قاعدة البيانات
echo - واجهة محسنة مع معلومات التطبيق
echo.
echo [INFO] سيتم تشغيل التطبيق الآن...
echo.
pause

:: تشغيل التطبيق
start "" "%APP_DIR%\SmartInventoryPro.exe"


