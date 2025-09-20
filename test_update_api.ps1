# Test GitHub API for updates
Write-Host "Testing GitHub API for updates..." -ForegroundColor Cyan

try {
    # Get latest release from GitHub
    $githubUrl = "https://api.github.com/repos/Hadani0mar/InfinityPOS/releases/latest"
    Write-Host "Connecting to: $githubUrl" -ForegroundColor Yellow
    
    $response = Invoke-RestMethod -Uri $githubUrl -Headers @{
        "User-Agent" = "SmartInventoryPro-Updater"
    }
    
    Write-Host "Data retrieved successfully!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Latest release info:" -ForegroundColor Cyan
    Write-Host "Tag: $($response.tag_name)" -ForegroundColor White
    Write-Host "Published: $($response.published_at)" -ForegroundColor White
    Write-Host "Description: $($response.body)" -ForegroundColor White
    Write-Host ""
    
    # Check attached files
    if ($response.assets -and $response.assets.Count -gt 0) {
        Write-Host "Attached files:" -ForegroundColor Cyan
        foreach ($asset in $response.assets) {
            Write-Host "  File: $($asset.name) - $($asset.browser_download_url)" -ForegroundColor White
        }
    } else {
        Write-Host "No attached files found!" -ForegroundColor Red
    }
    
    Write-Host ""
    Write-Host "Checking current version..." -ForegroundColor Cyan
    
    # Read current version from appsettings.json
    $appSettingsPath = "appsettings.json"
    if (Test-Path $appSettingsPath) {
        $appSettings = Get-Content $appSettingsPath | ConvertFrom-Json
        $currentVersion = $appSettings.Version
        Write-Host "Current version: $currentVersion" -ForegroundColor White
    } else {
        Write-Host "appsettings.json not found!" -ForegroundColor Red
        $currentVersion = "1.0.0"
    }
    
    # Compare versions
    $latestVersion = $response.tag_name -replace "v", "" -replace "V", ""
    Write-Host "Latest version: $latestVersion" -ForegroundColor White
    
    if ($latestVersion -gt $currentVersion) {
        Write-Host "Update available!" -ForegroundColor Green
    } else {
        Write-Host "Application is up to date!" -ForegroundColor Green
    }
    
} catch {
    Write-Host "Connection error: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Error details: $($_.Exception)" -ForegroundColor Red
}

Write-Host ""
Write-Host "Test completed" -ForegroundColor Cyan
Read-Host "Press Enter to exit"
