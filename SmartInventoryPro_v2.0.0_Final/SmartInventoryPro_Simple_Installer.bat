@echo off
setlocal EnableDelayedExpansion
title SmartInventory Pro v2.0.0 - Universal Installer

echo ==========================================
echo    SmartInventory Pro v2.0.0
echo    Universal Installer
echo ==========================================
echo.

REM Check for admin privileges
net session >nul 2>&1
if %errorlevel% neq 0 (
    echo This installer needs administrator privileges.
    echo Please run as administrator.
    echo.
    pause
    exit /b 1
)

echo [1/6] Preparing installation...

REM Set installation directory
set "INSTALL_DIR=%ProgramFiles%\SmartInventory Pro"
set "APP_DATA_DIR=%APPDATA%\SmartInventory Pro"
set "DESKTOP_DIR=%USERPROFILE%\Desktop"
set "START_MENU_DIR=%APPDATA%\Microsoft\Windows\Start Menu\Programs"

echo [2/6] Checking for existing installation...

REM Stop any running instances
taskkill /f /im SmartInventoryPro.exe >nul 2>&1

REM Remove old installation if exists
if exist "%INSTALL_DIR%" (
    echo Removing previous installation...
    
    REM Backup settings if they exist
    if exist "%INSTALL_DIR%\appsettings.json" (
        if not exist "%APP_DATA_DIR%" mkdir "%APP_DATA_DIR%"
        copy "%INSTALL_DIR%\appsettings.json" "%APP_DATA_DIR%\appsettings_backup.json" >nul 2>&1
    )
    
    rmdir /s /q "%INSTALL_DIR%" >nul 2>&1
)

echo [3/6] Creating installation directory...

REM Create installation directory
if not exist "%INSTALL_DIR%" mkdir "%INSTALL_DIR%"
if not exist "%APP_DATA_DIR%" mkdir "%APP_DATA_DIR%"

echo [4/6] Copying application files...

REM Copy all files from current directory to installation directory
copy /Y "SmartInventoryPro.exe" "%INSTALL_DIR%\" >nul 2>&1
if %errorlevel% neq 0 (
    echo ERROR: Failed to copy SmartInventoryPro.exe
    echo Make sure the file exists in the current directory.
    pause
    exit /b 1
)

REM Copy or restore settings
if exist "%APP_DATA_DIR%\appsettings_backup.json" (
    copy /Y "%APP_DATA_DIR%\appsettings_backup.json" "%INSTALL_DIR%\appsettings.json" >nul 2>&1
    echo Settings restored from backup.
) else if exist "appsettings.json" (
    copy /Y "appsettings.json" "%INSTALL_DIR%\" >nul 2>&1
) else (
    REM Create default settings file
    echo {> "%INSTALL_DIR%\appsettings.json"
    echo   "ConnectionStrings": {>> "%INSTALL_DIR%\appsettings.json"
    echo     "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=pharmacy1;User Id=sa;Password=123;TrustServerCertificate=true;MultipleActiveResultSets=true;Encrypt=false;ConnectRetryCount=3;ConnectRetryInterval=10">> "%INSTALL_DIR%\appsettings.json"
    echo   },>> "%INSTALL_DIR%\appsettings.json"
    echo   "Logging": {>> "%INSTALL_DIR%\appsettings.json"
    echo     "LogLevel": {>> "%INSTALL_DIR%\appsettings.json"
    echo       "Default": "Information">> "%INSTALL_DIR%\appsettings.json"
    echo     }>> "%INSTALL_DIR%\appsettings.json"
    echo   },>> "%INSTALL_DIR%\appsettings.json"
    echo   "Application": {>> "%INSTALL_DIR%\appsettings.json"
    echo     "Name": "SmartInventory Pro",>> "%INSTALL_DIR%\appsettings.json"
    echo     "Version": "2.0.0",>> "%INSTALL_DIR%\appsettings.json"
    echo     "Environment": "Production">> "%INSTALL_DIR%\appsettings.json"
    echo   }>> "%INSTALL_DIR%\appsettings.json"
    echo }>> "%INSTALL_DIR%\appsettings.json"
)

echo [5/6] Creating shortcuts...

REM Create desktop shortcut
echo Creating desktop shortcut...
powershell -Command "$WshShell = New-Object -comObject WScript.Shell; $Shortcut = $WshShell.CreateShortcut('%DESKTOP_DIR%\SmartInventory Pro.lnk'); $Shortcut.TargetPath = '%INSTALL_DIR%\SmartInventoryPro.exe'; $Shortcut.WorkingDirectory = '%INSTALL_DIR%'; $Shortcut.Description = 'SmartInventory Pro v2.0.0'; $Shortcut.Save()" >nul 2>&1

REM Create start menu shortcut
echo Creating start menu shortcut...
powershell -Command "$WshShell = New-Object -comObject WScript.Shell; $Shortcut = $WshShell.CreateShortcut('%START_MENU_DIR%\SmartInventory Pro.lnk'); $Shortcut.TargetPath = '%INSTALL_DIR%\SmartInventoryPro.exe'; $Shortcut.WorkingDirectory = '%INSTALL_DIR%'; $Shortcut.Description = 'SmartInventory Pro v2.0.0'; $Shortcut.Save()" >nul 2>&1

echo [6/6] Final setup...

REM Add to Programs and Features (optional)
reg add "HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\SmartInventoryPro" /v "DisplayName" /t REG_SZ /d "SmartInventory Pro v2.0.0" /f >nul 2>&1
reg add "HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\SmartInventoryPro" /v "DisplayVersion" /t REG_SZ /d "2.0.0" /f >nul 2>&1
reg add "HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\SmartInventoryPro" /v "Publisher" /t REG_SZ /d "SmartInventory Pro Team" /f >nul 2>&1
reg add "HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\SmartInventoryPro" /v "InstallLocation" /t REG_SZ /d "%INSTALL_DIR%" /f >nul 2>&1

echo.
echo ==========================================
echo Installation completed successfully!
echo ==========================================
echo.
echo Installation path: %INSTALL_DIR%
echo Desktop shortcut: Created
echo Start menu shortcut: Created
echo.
echo You can now launch SmartInventory Pro from:
echo - Desktop shortcut
echo - Start menu
echo - Or directly from: %INSTALL_DIR%\SmartInventoryPro.exe
echo.

REM Ask if user wants to launch the application
set /p launch="Do you want to launch SmartInventory Pro now? (Y/N): "
if /i "!launch!"=="Y" (
    echo Launching SmartInventory Pro...
    start "" "%INSTALL_DIR%\SmartInventoryPro.exe"
)

echo.
echo Thank you for installing SmartInventory Pro!
pause
