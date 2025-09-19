# SmartInventory Pro - Advanced Update Installer
# PowerShell script for automatic updates

param(
    [string]$UpdateUrl = "",
    [string]$InstallPath = "",
    [switch]$Silent = $false,
    [switch]$Force = $false
)

# إعدادات التحديث
$AppName = "SmartInventory Pro"
$AppExe = "SmartInventoryPro.exe"
$BackupPath = "$env:TEMP\SmartInventoryPro_Backup"
$LogFile = "$env:TEMP\SmartInventoryPro_Update.log"

# دالة كتابة السجل
function Write-Log {
    param([string]$Message)
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $logMessage = "[$timestamp] $Message"
    Write-Host $logMessage
    Add-Content -Path $LogFile -Value $logMessage
}

# دالة فحص العمليات النشطة
function Test-AppRunning {
    $processes = Get-Process -Name "SmartInventoryPro" -ErrorAction SilentlyContinue
    return $processes.Count -gt 0
}

# دالة إيقاف التطبيق
function Stop-App {
    Write-Log "محاولة إيقاف التطبيق..."
    
    $maxAttempts = 10
    $attempt = 0
    
    while (Test-AppRunning -and $attempt -lt $maxAttempts) {
        $attempt++
        Write-Log "محاولة $attempt من $maxAttempts لإيقاف التطبيق..."
        
        Get-Process -Name "SmartInventoryPro" -ErrorAction SilentlyContinue | Stop-Process -Force
        Start-Sleep -Seconds 2
    }
    
    if (Test-AppRunning) {
        Write-Log "تحذير: لم يتم إيقاف التطبيق بالكامل"
        return $false
    }
    
    Write-Log "تم إيقاف التطبيق بنجاح"
    return $true
}

# دالة إنشاء نسخة احتياطية
function Backup-CurrentVersion {
    Write-Log "إنشاء نسخة احتياطية..."
    
    if (Test-Path $BackupPath) {
        Remove-Item -Path $BackupPath -Recurse -Force
    }
    
    New-Item -Path $BackupPath -ItemType Directory -Force | Out-Null
    Copy-Item -Path "$InstallPath\*" -Destination $BackupPath -Recurse -Force
    
    Write-Log "تم إنشاء النسخة الاحتياطية في: $BackupPath"
}

# دالة استعادة النسخة الاحتياطية
function Restore-Backup {
    Write-Log "استعادة النسخة الاحتياطية..."
    
    if (Test-Path $BackupPath) {
        Copy-Item -Path "$BackupPath\*" -Destination $InstallPath -Recurse -Force
        Write-Log "تم استعادة النسخة الاحتياطية"
        return $true
    }
    
    Write-Log "خطأ: لم يتم العثور على النسخة الاحتياطية"
    return $false
}

# دالة تحميل التحديث
function Download-Update {
    param([string]$Url)
    
    Write-Log "تحميل التحديث من: $Url"
    
    $tempFile = "$env:TEMP\SmartInventoryPro_Update.zip"
    
    try {
        Invoke-WebRequest -Uri $Url -OutFile $tempFile -UseBasicParsing
        Write-Log "تم تحميل التحديث بنجاح"
        return $tempFile
    }
    catch {
        Write-Log "خطأ في تحميل التحديث: $($_.Exception.Message)"
        return $null
    }
}

# دالة استخراج التحديث
function Extract-Update {
    param([string]$ZipFile)
    
    Write-Log "استخراج التحديث..."
    
    $extractPath = "$env:TEMP\SmartInventoryPro_Extract"
    
    if (Test-Path $extractPath) {
        Remove-Item -Path $extractPath -Recurse -Force
    }
    
    try {
        Expand-Archive -Path $ZipFile -DestinationPath $extractPath -Force
        Write-Log "تم استخراج التحديث بنجاح"
        return $extractPath
    }
    catch {
        Write-Log "خطأ في استخراج التحديث: $($_.Exception.Message)"
        return $null
    }
}

# دالة تطبيق التحديث
function Apply-Update {
    param([string]$ExtractPath)
    
    Write-Log "تطبيق التحديث..."
    
    try {
        # نسخ الملفات الجديدة
        Copy-Item -Path "$ExtractPath\*" -Destination $InstallPath -Recurse -Force
        
        # تنظيف الملفات المؤقتة
        Remove-Item -Path $ExtractPath -Recurse -Force
        Remove-Item -Path "$env:TEMP\SmartInventoryPro_Update.zip" -Force -ErrorAction SilentlyContinue
        
        Write-Log "تم تطبيق التحديث بنجاح"
        return $true
    }
    catch {
        Write-Log "خطأ في تطبيق التحديث: $($_.Exception.Message)"
        return $false
    }
}

# دالة إعادة تشغيل التطبيق
function Start-App {
    Write-Log "إعادة تشغيل التطبيق..."
    
    $appPath = Join-Path $InstallPath $AppExe
    
    if (Test-Path $appPath) {
        Start-Process -FilePath $appPath -WorkingDirectory $InstallPath
        Write-Log "تم إعادة تشغيل التطبيق"
        return $true
    }
    else {
        Write-Log "خطأ: لم يتم العثور على ملف التطبيق"
        return $false
    }
}

# الدالة الرئيسية
function Main {
    Write-Log "بدء عملية تحديث $AppName"
    
    # فحص المعاملات
    if ([string]::IsNullOrEmpty($UpdateUrl)) {
        Write-Log "خطأ: يجب تحديد رابط التحديث"
        return 1
    }
    
    if ([string]::IsNullOrEmpty($InstallPath)) {
        $InstallPath = Split-Path -Parent $MyInvocation.MyCommand.Path
    }
    
    Write-Log "مسار التثبيت: $InstallPath"
    Write-Log "رابط التحديث: $UpdateUrl"
    
    try {
        # 1. إيقاف التطبيق
        if (-not (Stop-App)) {
            if (-not $Force) {
                Write-Log "فشل في إيقاف التطبيق. استخدم -Force لتجاهل هذا التحذير"
                return 1
            }
        }
        
        # 2. إنشاء نسخة احتياطية
        Backup-CurrentVersion
        
        # 3. تحميل التحديث
        $zipFile = Download-Update -Url $UpdateUrl
        if (-not $zipFile) {
            Write-Log "فشل في تحميل التحديث"
            return 1
        }
        
        # 4. استخراج التحديث
        $extractPath = Extract-Update -ZipFile $zipFile
        if (-not $extractPath) {
            Write-Log "فشل في استخراج التحديث"
            Restore-Backup
            return 1
        }
        
        # 5. تطبيق التحديث
        if (-not (Apply-Update -ExtractPath $extractPath)) {
            Write-Log "فشل في تطبيق التحديث، استعادة النسخة الاحتياطية..."
            Restore-Backup
            return 1
        }
        
        # 6. إعادة تشغيل التطبيق
        if (-not $Silent) {
            Start-App
        }
        
        Write-Log "تم تحديث $AppName بنجاح!"
        return 0
    }
    catch {
        Write-Log "خطأ عام في التحديث: $($_.Exception.Message)"
        Restore-Backup
        return 1
    }
}

# تشغيل الدالة الرئيسية
$exitCode = Main
exit $exitCode
