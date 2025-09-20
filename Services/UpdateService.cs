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
                
                // إذا لم توجد تحديثات، أعد النتيجة بدون خطأ
                if (string.IsNullOrEmpty(githubUpdate.Error))
                {
                    var currentVersion = GetCurrentVersion();
                    return new UpdateInfo 
                    { 
                        HasUpdates = false, 
                        LocalCommit = currentVersion,
                        RemoteCommit = "latest",
                        LastMessage = "التطبيق محدث",
                        LastDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    };
                }

                // إذا فشل GitHub، أعد رسالة خطأ واضحة
                var errorMessage = "خطأ غير معروف";
                if (!string.IsNullOrEmpty(githubUpdate.Error))
                {
                    errorMessage = $"خطأ في الاتصال: {githubUpdate.Error}";
                }
                return new UpdateInfo
                {
                    HasUpdates = false,
                    Error = errorMessage
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"خطأ في CheckForUpdatesAsync: {ex.Message}");
                var exceptionMessage = ex.Message ?? "خطأ غير معروف";
                return new UpdateInfo
                {
                    HasUpdates = false,
                    Error = $"خطأ في الاتصال: {exceptionMessage}"
                };
            }
        }

        private async Task<UpdateInfo> CheckGitHubUpdatesAsync()
        {
            try
            {
                var response = await _httpClient.GetStringAsync($"{GITHUB_REPO}/releases/latest");
                var release = JsonConvert.DeserializeObject<GitHubRelease>(response);
                
                if (release == null || string.IsNullOrEmpty(release.TagName))
                {
                    return new UpdateInfo { HasUpdates = false, Error = "No release data found" };
                }
                
                var currentVersion = GetCurrentVersion();
                var latestVersion = "1.0.0";
                if (!string.IsNullOrEmpty(release.TagName))
                {
                    latestVersion = release.TagName.Replace("v", "").Replace("V", "");
                }
                
                // حماية إضافية من القيم الفارغة
                var lastMessage = !string.IsNullOrEmpty(release.Body) ? release.Body : "تحديث جديد متاح";
                var lastDate = release.PublishedAt != default ? release.PublishedAt.ToString("yyyy-MM-dd HH:mm:ss") : DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                var lastHash = "unknown";
                if (!string.IsNullOrEmpty(release.TagName))
                {
                    lastHash = release.TagName;
                }
                var downloadUrl = "";
                if (release.Assets != null && release.Assets.Length > 0)
                {
                    var firstAsset = release.Assets.FirstOrDefault();
                    if (firstAsset != null && !string.IsNullOrEmpty(firstAsset.BrowserDownloadUrl))
                    {
                        downloadUrl = firstAsset.BrowserDownloadUrl;
                    }
                }

                return new UpdateInfo
                {
                    HasUpdates = IsNewerVersion(latestVersion, currentVersion),
                    LocalCommit = currentVersion,
                    RemoteCommit = latestVersion,
                    LastMessage = lastMessage,
                    LastDate = lastDate,
                    LastHash = lastHash,
                    DownloadUrl = downloadUrl
                };
            }
            catch (HttpRequestException ex)
            {
                System.Diagnostics.Debug.WriteLine($"خطأ في الاتصال بـ GitHub: {ex.Message}");
                return new UpdateInfo { HasUpdates = false, Error = $"خطأ في الاتصال: {ex.Message}" };
            }
            catch (JsonException ex)
            {
                System.Diagnostics.Debug.WriteLine($"خطأ في تحليل JSON: {ex.Message}");
                return new UpdateInfo { HasUpdates = false, Error = $"خطأ في تحليل البيانات: {ex.Message}" };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"خطأ عام في جلب التحديثات من GitHub: {ex.Message}");
                return new UpdateInfo { HasUpdates = false, Error = ex.Message };
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
                if (string.IsNullOrEmpty(downloadUrl))
                {
                    return new UpdateResult
                    {
                        Success = false,
                        Error = "رابط التحميل فارغ"
                    };
                }

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
            catch (HttpRequestException ex)
            {
                System.Diagnostics.Debug.WriteLine($"خطأ في تحميل التحديث: {ex.Message}");
                return new UpdateResult
                {
                    Success = false,
                    Error = $"خطأ في تحميل التحديث: {ex.Message}"
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"خطأ عام في DownloadAndApplyUpdateAsync: {ex.Message}");
                return new UpdateResult
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        private string CreateUpdateScript(string updatePath)
        {
            try
            {
                // استخدام AppContext.BaseDirectory للـ single-file apps
                var currentPath = AppContext.BaseDirectory;
                if (string.IsNullOrEmpty(currentPath))
                {
                    currentPath = Environment.CurrentDirectory;
                }
                if (string.IsNullOrEmpty(currentPath))
                {
                    currentPath = Directory.GetCurrentDirectory();
                }
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

REM التأكد من وجود ملف appsettings.json
if not exist ""{currentPath}\appsettings.json"" (
    echo تحذير: ملف appsettings.json مفقود!
    echo إنشاء ملف إعدادات افتراضي...
    echo {{}} > ""{currentPath}\appsettings.json""
)

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
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"خطأ في CreateUpdateScript: {ex.Message}");
                return $@"
@echo off
echo خطأ في إنشاء سكريبت التحديث: {ex.Message}
pause
";
            }
        }

        private string GetCurrentVersion()
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                if (assembly?.GetName()?.Version != null)
                {
                    var version = assembly.GetName().Version;
                    if (version != null)
                    {
                        return $"{version.Major}.{version.Minor}.{version.Build}";
                    }
                }
                return "1.0.0";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"خطأ في جلب الإصدار: {ex.Message}");
                return "1.0.0";
            }
        }

        private bool IsNewerVersion(string latest, string current)
        {
            try
            {
                if (string.IsNullOrEmpty(latest) || string.IsNullOrEmpty(current))
                    return false;

                var latestParts = latest.Split('.').Where(x => !string.IsNullOrEmpty(x)).Select(x => int.TryParse(x, out var num) ? num : 0).ToArray();
                var currentParts = current.Split('.').Where(x => !string.IsNullOrEmpty(x)).Select(x => int.TryParse(x, out var num) ? num : 0).ToArray();

                if (latestParts.Length == 0 || currentParts.Length == 0)
                    return false;

                for (int i = 0; i < Math.Max(latestParts.Length, currentParts.Length); i++)
                {
                    var latestPart = i < latestParts.Length ? latestParts[i] : 0;
                    var currentPart = i < currentParts.Length ? currentParts[i] : 0;

                    if (latestPart > currentPart) return true;
                    if (latestPart < currentPart) return false;
                }
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"خطأ في مقارنة الإصدارات: {ex.Message}");
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
