using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Reflection;
using Newtonsoft.Json;

namespace SmartInventoryPro.Services
{
    public class UpdateService
    {
        private readonly HttpClient _httpClient;
        private const string API_BASE_URL = "http://102.213.180.199:8080/infinitypos-api/";
        private const string GITHUB_REPO = "https://api.github.com/repos/Hadani0mar/InfinityPOS";
        private const string DOWNLOAD_URL = "https://github.com/Hadani0mar/InfinityPOS/releases/latest/download/";

        public UpdateService()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "SmartInventoryPro-Updater");
        }

        public async Task<UpdateInfo> CheckForUpdatesAsync()
        {
            try
            {
                // محاولة جلب التحديثات من GitHub أولاً
                var githubUpdate = await CheckGitHubUpdatesAsync();
                if (githubUpdate.HasUpdates)
                    return githubUpdate;

                // إذا فشل GitHub، جرب الخادم المحلي
                var response = await _httpClient.GetStringAsync($"{API_BASE_URL}check-updates.php");
                return JsonConvert.DeserializeObject<UpdateInfo>(response);
            }
            catch (Exception ex)
            {
                return new UpdateInfo
                {
                    HasUpdates = false,
                    Error = ex.Message
                };
            }
        }

        private async Task<UpdateInfo> CheckGitHubUpdatesAsync()
        {
            try
            {
                var response = await _httpClient.GetStringAsync($"{GITHUB_REPO}/releases/latest");
                var release = JsonConvert.DeserializeObject<GitHubRelease>(response);
                
                var currentVersion = GetCurrentVersion();
                var latestVersion = release.TagName.Replace("v", "");

                return new UpdateInfo
                {
                    HasUpdates = IsNewerVersion(latestVersion, currentVersion),
                    LocalCommit = currentVersion,
                    RemoteCommit = latestVersion,
                    LastMessage = release.Body,
                    LastDate = release.PublishedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                    DownloadUrl = release.Assets?.FirstOrDefault()?.BrowserDownloadUrl
                };
            }
            catch
            {
                return new UpdateInfo { HasUpdates = false };
            }
        }

        public async Task<UpdateResult> ApplyUpdateAsync()
        {
            try
            {
                // تحميل التحديث من GitHub
                var updateInfo = await CheckForUpdatesAsync();
                if (updateInfo.HasUpdates && !string.IsNullOrEmpty(updateInfo.DownloadUrl))
                {
                    return await DownloadAndApplyUpdateAsync(updateInfo.DownloadUrl);
                }

                // إذا فشل، جرب الخادم المحلي
                var response = await _httpClient.GetStringAsync($"{API_BASE_URL}apply-update.php");
                return JsonConvert.DeserializeObject<UpdateResult>(response);
            }
            catch (Exception ex)
            {
                return new UpdateResult
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        private async Task<UpdateResult> DownloadAndApplyUpdateAsync(string downloadUrl)
        {
            try
            {
                var tempPath = Path.Combine(Path.GetTempPath(), "SmartInventoryPro_Update.zip");
                var updatePath = Path.Combine(Path.GetTempPath(), "SmartInventoryPro_Update");

                // تحميل الملف
                var fileBytes = await _httpClient.GetByteArrayAsync(downloadUrl);
                await File.WriteAllBytesAsync(tempPath, fileBytes);

                // استخراج الملف
                System.IO.Compression.ZipFile.ExtractToDirectory(tempPath, updatePath);

                // إنشاء سكريبت التحديث
                var updateScript = CreateUpdateScript(updatePath);
                var scriptPath = Path.Combine(Path.GetTempPath(), "Update_SmartInventoryPro.bat");
                await File.WriteAllTextAsync(scriptPath, updateScript);

                // تشغيل سكريبت التحديث
                Process.Start(new ProcessStartInfo
                {
                    FileName = scriptPath,
                    UseShellExecute = true,
                    Verb = "runas" // تشغيل كمدير
                });

                return new UpdateResult
                {
                    Success = true,
                    NewMessage = "تم بدء عملية التحديث. سيتم إعادة تشغيل التطبيق تلقائياً."
                };
            }
            catch (Exception ex)
            {
                return new UpdateResult
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        private string CreateUpdateScript(string updatePath)
        {
            var currentPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var appName = "SmartInventoryPro.exe";

            return $@"
@echo off
echo تحديث SmartInventory Pro...
echo.

REM انتظار إغلاق التطبيق
timeout /t 3 /nobreak >nul

REM نسخ الملفات الجديدة
echo نسخ الملفات الجديدة...
xcopy ""{updatePath}\*"" ""{currentPath}\"" /Y /E /I

REM تنظيف الملفات المؤقتة
echo تنظيف الملفات المؤقتة...
rmdir /s /q ""{updatePath}""
del ""{Path.Combine(Path.GetTempPath(), "SmartInventoryPro_Update.zip")}""

REM إعادة تشغيل التطبيق
echo إعادة تشغيل التطبيق...
start """" ""{Path.Combine(currentPath, appName)}""

REM حذف سكريبت التحديث
del ""%~f0""

echo تم التحديث بنجاح!
pause
";
        }

        private string GetCurrentVersion()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            return $"{version.Major}.{version.Minor}.{version.Build}";
        }

        private bool IsNewerVersion(string latest, string current)
        {
            try
            {
                var latestParts = latest.Split('.').Select(int.Parse).ToArray();
                var currentParts = current.Split('.').Select(int.Parse).ToArray();

                for (int i = 0; i < Math.Max(latestParts.Length, currentParts.Length); i++)
                {
                    var latestPart = i < latestParts.Length ? latestParts[i] : 0;
                    var currentPart = i < currentParts.Length ? currentParts[i] : 0;

                    if (latestPart > currentPart) return true;
                    if (latestPart < currentPart) return false;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
    }

    public class UpdateInfo
    {
        public bool HasUpdates { get; set; }
        public string LocalCommit { get; set; } = string.Empty;
        public string RemoteCommit { get; set; } = string.Empty;
        public string LastMessage { get; set; } = string.Empty;
        public string LastHash { get; set; } = string.Empty;
        public string LastDate { get; set; } = string.Empty;
        public string Timestamp { get; set; } = string.Empty;
        public string Error { get; set; } = string.Empty;
        public string DownloadUrl { get; set; } = string.Empty;
    }

    public class UpdateResult
    {
        public bool Success { get; set; }
        public string NewCommit { get; set; } = string.Empty;
        public string NewMessage { get; set; } = string.Empty;
        public string NewDate { get; set; } = string.Empty;
        public string Error { get; set; } = string.Empty;
    }

    public class GitHubRelease
    {
        [JsonProperty("tag_name")]
        public string TagName { get; set; } = string.Empty;
        
        [JsonProperty("body")]
        public string Body { get; set; } = string.Empty;
        
        [JsonProperty("published_at")]
        public DateTime PublishedAt { get; set; }
        
        [JsonProperty("assets")]
        public GitHubAsset[] Assets { get; set; } = Array.Empty<GitHubAsset>();
    }

    public class GitHubAsset
    {
        [JsonProperty("browser_download_url")]
        public string BrowserDownloadUrl { get; set; } = string.Empty;
        
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;
    }
}
