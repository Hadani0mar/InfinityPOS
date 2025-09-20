# Create EXE installer for SmartInventory Pro
Write-Host "🚀 إنشاء مثبت EXE لـ SmartInventory Pro..." -ForegroundColor Green

# Create a simple EXE installer using PowerShell
$installerCode = @'
using System;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;

public class SmartInventoryInstaller
{
    [STAThread]
    public static void Main()
    {
        try
        {
            string appName = "SmartInventory Pro";
            string appDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), appName);
            string startMenu = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Microsoft\Windows\Start Menu\Programs");
            string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            
            Console.WriteLine("========================================");
            Console.WriteLine("   SmartInventory Pro Installer v1.5.1");
            Console.WriteLine("========================================");
            Console.WriteLine();
            
            Console.WriteLine("[INFO] تثبيت " + appName + "...");
            Console.WriteLine();
            
            // Create application directory
            if (!Directory.Exists(appDir))
            {
                Directory.CreateDirectory(appDir);
                Console.WriteLine("[SUCCESS] تم إنشاء مجلد التطبيق");
            }
            
            // Copy application files
            Console.WriteLine("[INFO] نسخ ملفات التطبيق...");
            string sourceDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "publish");
            if (Directory.Exists(sourceDir))
            {
                CopyDirectory(sourceDir, appDir, true);
                Console.WriteLine("[SUCCESS] تم نسخ ملفات التطبيق");
            }
            else
            {
                Console.WriteLine("[ERROR] ملفات التطبيق غير موجودة");
                Console.WriteLine("اضغط أي مفتاح للخروج...");
                Console.ReadKey();
                return;
            }
            
            // Create shortcuts
            CreateShortcut(Path.Combine(startMenu, appName + ".lnk"), Path.Combine(appDir, "SmartInventoryPro.exe"), appDir);
            CreateShortcut(Path.Combine(desktop, appName + ".lnk"), Path.Combine(appDir, "SmartInventoryPro.exe"), appDir);
            
            Console.WriteLine();
            Console.WriteLine("========================================");
            Console.WriteLine("    تم التثبيت بنجاح!");
            Console.WriteLine("========================================");
            Console.WriteLine();
            Console.WriteLine("تم تثبيت " + appName + " في: " + appDir);
            Console.WriteLine("تم إنشاء اختصارات في قائمة ابدأ وسطح المكتب");
            Console.WriteLine();
            Console.WriteLine("اضغط أي مفتاح للخروج...");
            Console.ReadKey();
        }
        catch (Exception ex)
        {
            Console.WriteLine("[ERROR] خطأ في التثبيت: " + ex.Message);
            Console.WriteLine("اضغط أي مفتاح للخروج...");
            Console.ReadKey();
        }
    }
    
    private static void CopyDirectory(string sourceDir, string destDir, bool copySubDirs)
    {
        DirectoryInfo dir = new DirectoryInfo(sourceDir);
        if (!dir.Exists)
            throw new DirectoryNotFoundException("Source directory does not exist: " + sourceDir);
        
        DirectoryInfo[] dirs = dir.GetDirectories();
        if (!Directory.Exists(destDir))
            Directory.CreateDirectory(destDir);
        
        FileInfo[] files = dir.GetFiles();
        foreach (FileInfo file in files)
        {
            string targetPath = Path.Combine(destDir, file.Name);
            file.CopyTo(targetPath, true);
        }
        
        if (copySubDirs)
        {
            foreach (DirectoryInfo subdir in dirs)
            {
                string targetPath = Path.Combine(destDir, subdir.Name);
                CopyDirectory(subdir.FullName, targetPath, copySubDirs);
            }
        }
    }
    
    private static void CreateShortcut(string shortcutPath, string targetPath, string workingDirectory)
    {
        try
        {
            Type t = Type.GetTypeFromProgID("WScript.Shell");
            dynamic shell = Activator.CreateInstance(t);
            dynamic shortcut = shell.CreateShortcut(shortcutPath);
            shortcut.TargetPath = targetPath;
            shortcut.WorkingDirectory = workingDirectory;
            shortcut.Description = "SmartInventory Pro - Advanced Business Management System";
            shortcut.Save();
        }
        catch (Exception ex)
        {
            Console.WriteLine("[WARNING] فشل في إنشاء الاختصار: " + ex.Message);
        }
    }
}
'@

# Save the installer code
$installerCode | Out-File -FilePath "dist\Installer.cs" -Encoding UTF8

# Compile to EXE
Write-Host "🔨 تجميع المثبت..." -ForegroundColor Blue
$cscPath = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\2019\BuildTools\MSBuild\Current\Bin\Roslyn\csc.exe"
if (-not (Test-Path $cscPath)) {
    $cscPath = "${env:ProgramFiles}\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\Roslyn\csc.exe"
}
if (-not (Test-Path $cscPath)) {
    $cscPath = "csc.exe"
}

& $cscPath /target:exe /out:"dist\SmartInventoryPro_Setup_v1.5.1.exe" /reference:System.dll,System.Windows.Forms.dll "dist\Installer.cs"

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ تم إنشاء المثبت بنجاح!" -ForegroundColor Green
    Write-Host "📁 الملف: dist\SmartInventoryPro_Setup_v1.5.1.exe" -ForegroundColor Cyan
} else {
    Write-Host "❌ فشل في إنشاء المثبت" -ForegroundColor Red
    Write-Host "سيتم استخدام المثبت النصي بدلاً من ذلك" -ForegroundColor Yellow
}
