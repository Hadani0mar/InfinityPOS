# PowerShell script to create GitHub Release manually
# This script will create a release on GitHub with the built files

param(
    [string]$Token = "",
    [string]$Repo = "Hadani0mar/InfinityPOS",
    [string]$Tag = "v1.0.0",
    [string]$Name = "SmartInventory Pro v1.0.0 - First Official Release",
    [string]$Body = ""
)

if ([string]::IsNullOrEmpty($Token)) {
    Write-Host "❌ GitHub Token مطلوب!" -ForegroundColor Red
    Write-Host "يرجى إدخال GitHub Token:" -ForegroundColor Yellow
    $Token = Read-Host
}

if ([string]::IsNullOrEmpty($Body)) {
    $Body = @"
## 🎉 SmartInventory Pro v1.0.0 - First Official Release

### ✨ New Features:
- 🔥 **Fire icons** added to Top Product and Top Category cards
- 🚀 **Automatic update system** with GitHub integration
- 💾 **Database connection persistence** - saves login settings
- 🛠️ **Professional installer** with version management
- 📱 **Portable version** available for direct execution

### 📦 What's Included:
- `SmartInventoryPro.exe` - Main application (portable)
- `SmartInventoryPro_Setup_v1.0.0.exe` - Professional installer
- `appsettings.json` - Configuration file
- `README.txt` - Installation guide

### 🚀 Installation Options:
1. **Installer**: Run `SmartInventoryPro_Setup_v1.0.0.exe` for full installation
2. **Portable**: Run `SmartInventoryPro.exe` directly (no installation needed)

### 🔧 Technical Improvements:
- Fixed 'Index and length' error completely
- Enhanced error handling and logging
- Improved update mechanism
- Better database connection management

### 📋 System Requirements:
- Windows 10/11 (64-bit)
- .NET 8.0 Runtime (included in installer)
- SQL Server connection

---
**Download and enjoy the first official release of SmartInventory Pro!** 🎊
"@
}

# Create release data
$releaseData = @{
    tag_name = $Tag
    name = $Name
    body = $Body
    draft = $false
    prerelease = $false
} | ConvertTo-Json

Write-Host "🚀 إنشاء Release على GitHub..." -ForegroundColor Green

try {
    # Create release
    $headers = @{
        "Authorization" = "token $Token"
        "Accept" = "application/vnd.github.v3+json"
    }
    
    $response = Invoke-RestMethod -Uri "https://api.github.com/repos/$Repo/releases" -Method Post -Headers $headers -Body $releaseData -ContentType "application/json"
    
    Write-Host "✅ تم إنشاء Release بنجاح!" -ForegroundColor Green
    Write-Host "🔗 رابط Release: $($response.html_url)" -ForegroundColor Cyan
    
    # Upload files
    $uploadUrl = $response.upload_url -replace "\{.*\}", ""
    
    # Upload main zip file
    if (Test-Path "SmartInventoryPro_v1.0.0_Release.zip") {
        Write-Host "📤 رفع ملف ZIP..." -ForegroundColor Blue
        $fileBytes = [System.IO.File]::ReadAllBytes("SmartInventoryPro_v1.0.0_Release.zip")
        $fileEnc = [System.Text.Encoding]::GetEncoding('UTF-8').GetString($fileBytes)
        $boundary = [System.Guid]::NewGuid().ToString()
        $LF = "`r`n"
        
        $bodyLines = (
            "--$boundary",
            "Content-Disposition: form-data; name=`"file`"; filename=`"SmartInventoryPro_v1.0.0_Release.zip`"",
            "Content-Type: application/zip$LF",
            $fileEnc,
            "--$boundary--$LF"
        ) -join $LF
        
        $uploadHeaders = @{
            "Authorization" = "token $Token"
            "Content-Type" = "multipart/form-data; boundary=$boundary"
        }
        
        $uploadResponse = Invoke-RestMethod -Uri "$uploadUrl?name=SmartInventoryPro_v1.0.0_Release.zip" -Method Post -Headers $uploadHeaders -Body $bodyLines
        
        Write-Host "✅ تم رفع ملف ZIP بنجاح!" -ForegroundColor Green
    }
    
    Write-Host "🎉 تم إنشاء Release كاملاً!" -ForegroundColor Green
    Write-Host "🔗 يمكنك الآن تحميل الإصدار من: $($response.html_url)" -ForegroundColor Cyan
    
} catch {
    Write-Host "❌ خطأ في إنشاء Release: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "تأكد من صحة GitHub Token" -ForegroundColor Yellow
}
