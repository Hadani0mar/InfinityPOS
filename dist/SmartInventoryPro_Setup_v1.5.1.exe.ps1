# SmartInventory Pro Installer v1.5.1
# This creates a proper Windows installer

Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing

# Create installer form
$form = New-Object System.Windows.Forms.Form
$form.Text = "SmartInventory Pro Installer v1.5.1"
$form.Size = New-Object System.Drawing.Size(500, 400)
$form.StartPosition = "CenterScreen"
$form.FormBorderStyle = "FixedDialog"
$form.MaximizeBox = $false
$form.MinimizeBox = $false

# Add welcome label
$lblWelcome = New-Object System.Windows.Forms.Label
$lblWelcome.Text = "مرحباً بك في SmartInventory Pro"
$lblWelcome.Font = New-Object System.Drawing.Font("Segoe UI", 14, [System.Drawing.FontStyle]::Bold)
$lblWelcome.Location = New-Object System.Drawing.Point(20, 20)
$lblWelcome.Size = New-Object System.Drawing.Size(450, 30)
$lblWelcome.TextAlign = "MiddleCenter"
$form.Controls.Add($lblWelcome)

# Add description label
$lblDesc = New-Object System.Windows.Forms.Label
$lblDesc.Text = "سيقوم هذا المعالج بتثبيت SmartInventory Pro على جهازك.`n`nSmartInventory Pro هو نظام إدارة أعمال متقدم يوفر إدارة شاملة للمخزون والمبيعات."
$lblDesc.Font = New-Object System.Drawing.Font("Segoe UI", 10)
$lblDesc.Location = New-Object System.Drawing.Point(20, 60)
$lblDesc.Size = New-Object System.Drawing.Size(450, 80)
$lblDesc.TextAlign = "MiddleCenter"
$form.Controls.Add($lblDesc)

# Add install directory label
$lblDir = New-Object System.Windows.Forms.Label
$lblDir.Text = "مجلد التثبيت:"
$lblDir.Font = New-Object System.Drawing.Font("Segoe UI", 10)
$lblDir.Location = New-Object System.Drawing.Point(20, 160)
$lblDir.Size = New-Object System.Drawing.Size(100, 25)
$form.Controls.Add($lblDir)

# Add install directory textbox
$txtDir = New-Object System.Windows.Forms.TextBox
$txtDir.Text = "$env:PROGRAMFILES\SmartInventory Pro"
$txtDir.Font = New-Object System.Drawing.Font("Segoe UI", 10)
$txtDir.Location = New-Object System.Drawing.Point(130, 160)
$txtDir.Size = New-Object System.Drawing.Size(300, 25)
$form.Controls.Add($txtDir)

# Add browse button
$btnBrowse = New-Object System.Windows.Forms.Button
$btnBrowse.Text = "تصفح..."
$btnBrowse.Font = New-Object System.Drawing.Font("Segoe UI", 10)
$btnBrowse.Location = New-Object System.Drawing.Point(440, 160)
$btnBrowse.Size = New-Object System.Drawing.Size(80, 25)
$form.Controls.Add($btnBrowse)

# Add options
$chkStartMenu = New-Object System.Windows.Forms.CheckBox
$chkStartMenu.Text = "إنشاء اختصار في قائمة ابدأ"
$chkStartMenu.Font = New-Object System.Drawing.Font("Segoe UI", 10)
$chkStartMenu.Location = New-Object System.Drawing.Point(20, 200)
$chkStartMenu.Size = New-Object System.Drawing.Size(200, 25)
$chkStartMenu.Checked = $true
$form.Controls.Add($chkStartMenu)

$chkDesktop = New-Object System.Windows.Forms.CheckBox
$chkDesktop.Text = "إنشاء اختصار على سطح المكتب"
$chkDesktop.Font = New-Object System.Drawing.Font("Segoe UI", 10)
$chkDesktop.Location = New-Object System.Drawing.Point(20, 230)
$chkDesktop.Size = New-Object System.Drawing.Size(200, 25)
$chkDesktop.Checked = $true
$form.Controls.Add($chkDesktop)

# Add install button
$btnInstall = New-Object System.Windows.Forms.Button
$btnInstall.Text = "تثبيت"
$btnInstall.Font = New-Object System.Drawing.Font("Segoe UI", 12, [System.Drawing.FontStyle]::Bold)
$btnInstall.Location = New-Object System.Drawing.Point(200, 280)
$btnInstall.Size = New-Object System.Drawing.Size(100, 40)
$btnInstall.BackColor = [System.Drawing.Color]::FromArgb(52, 152, 219)
$btnInstall.ForeColor = [System.Drawing.Color]::White
$btnInstall.FlatStyle = "Flat"
$form.Controls.Add($btnInstall)

# Add cancel button
$btnCancel = New-Object System.Windows.Forms.Button
$btnCancel.Text = "إلغاء"
$btnCancel.Font = New-Object System.Drawing.Font("Segoe UI", 10)
$btnCancel.Location = New-Object System.Drawing.Point(320, 290)
$btnCancel.Size = New-Object System.Drawing.Size(80, 30)
$form.Controls.Add($btnCancel)

# Browse button click event
$btnBrowse.Add_Click({
    $folderBrowser = New-Object System.Windows.Forms.FolderBrowserDialog
    $folderBrowser.Description = "اختر مجلد التثبيت"
    $folderBrowser.SelectedPath = $txtDir.Text
    if ($folderBrowser.ShowDialog() -eq "OK") {
        $txtDir.Text = $folderBrowser.SelectedPath
    }
})

# Install button click event
$btnInstall.Add_Click({
    try {
        $btnInstall.Enabled = $false
        $btnInstall.Text = "جاري التثبيت..."
        
        $installDir = $txtDir.Text
        $startMenu = $chkStartMenu.Checked
        $desktop = $chkDesktop.Checked
        
        # Create installation directory
        if (-not (Test-Path $installDir)) {
            New-Item -ItemType Directory -Path $installDir -Force | Out-Null
        }
        
        # Copy application files
        $sourceDir = Join-Path $PSScriptRoot "publish"
        if (Test-Path $sourceDir) {
            Copy-Item "$sourceDir\*" $installDir -Recurse -Force
            Copy-Item "$PSScriptRoot\appsettings.json" $installDir -Force
            Copy-Item "$PSScriptRoot\DOWNLOAD_FROM_GITHUB.md" $installDir -Force
        } else {
            [System.Windows.Forms.MessageBox]::Show("ملفات التطبيق غير موجودة!", "خطأ", "OK", "Error")
            return
        }
        
        # Create shortcuts
        if ($startMenu) {
            $startMenuPath = "$env:APPDATA\Microsoft\Windows\Start Menu\Programs"
            $shortcutPath = Join-Path $startMenuPath "SmartInventory Pro.lnk"
            $WshShell = New-Object -comObject WScript.Shell
            $Shortcut = $WshShell.CreateShortcut($shortcutPath)
            $Shortcut.TargetPath = Join-Path $installDir "SmartInventoryPro.exe"
            $Shortcut.WorkingDirectory = $installDir
            $Shortcut.Description = "SmartInventory Pro - Advanced Business Management System"
            $Shortcut.Save()
        }
        
        if ($desktop) {
            $desktopPath = [Environment]::GetFolderPath("Desktop")
            $shortcutPath = Join-Path $desktopPath "SmartInventory Pro.lnk"
            $WshShell = New-Object -comObject WScript.Shell
            $Shortcut = $WshShell.CreateShortcut($shortcutPath)
            $Shortcut.TargetPath = Join-Path $installDir "SmartInventoryPro.exe"
            $Shortcut.WorkingDirectory = $installDir
            $Shortcut.Description = "SmartInventory Pro - Advanced Business Management System"
            $Shortcut.Save()
        }
        
        # Create uninstaller
        $uninstallerScript = @"
@echo off
chcp 65001 >nul
echo إلغاء تثبيت SmartInventory Pro...
taskkill /f /im SmartInventoryPro.exe 2>nul
timeout /t 2 /nobreak >nul
rmdir /s /q "$installDir"
del "$env:APPDATA\Microsoft\Windows\Start Menu\Programs\SmartInventory Pro.lnk" 2>nul
del "$env:USERPROFILE\Desktop\SmartInventory Pro.lnk" 2>nul
echo تم إلغاء التثبيت بنجاح!
pause
"@
        $uninstallerScript | Out-File -FilePath (Join-Path $installDir "Uninstall.bat") -Encoding UTF8
        
        [System.Windows.Forms.MessageBox]::Show("تم التثبيت بنجاح!", "نجح التثبيت", "OK", "Information")
        $form.Close()
    }
    catch {
        [System.Windows.Forms.MessageBox]::Show("خطأ في التثبيت: $($_.Exception.Message)", "خطأ", "OK", "Error")
        $btnInstall.Enabled = $true
        $btnInstall.Text = "تثبيت"
    }
})

# Cancel button click event
$btnCancel.Add_Click({
    $form.Close()
})

# Show form
$form.ShowDialog() | Out-Null
