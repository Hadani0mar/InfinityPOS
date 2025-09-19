using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using SmartInventoryPro.Data;
using SmartInventoryPro.Forms;
using System.Text;
using System.IO;
using System.Text.Json;

namespace SmartInventoryPro
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            // Register encoding providers for SQL Server compatibility
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            
            // Set up global exception handling
            Application.ThreadException += Application_ThreadException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            
            ApplicationConfiguration.Initialize();

            try
            {
                string? connectionString = null;
                
                // محاولة استخدام الإعدادات المحفوظة أولاً
                var savedSettings = LoadSavedDatabaseSettings();
                if (savedSettings != null && savedSettings.RememberSettings)
                {
                    connectionString = BuildConnectionString(savedSettings);
                    
                    // اختبار الاتصال بالإعدادات المحفوظة
                    if (TestConnection(connectionString))
                    {
                        // الاتصال ناجح، استخدم الإعدادات المحفوظة
                    }
                    else
                    {
                        // فشل الاتصال، اعرض نموذج الاتصال
                        connectionString = null;
                    }
                }
                
                // إذا لم تكن هناك إعدادات محفوظة أو فشل الاتصال
                if (string.IsNullOrEmpty(connectionString))
                {
                    var dbConnectionForm = new DatabaseConnectionForm();
                    if (dbConnectionForm.ShowDialog() == DialogResult.OK)
                    {
                        connectionString = dbConnectionForm.ConnectionString;
                    }
                    else
                    {
                        return; // المستخدم ألغى العملية
                    }
                }
                
                if (!string.IsNullOrEmpty(connectionString))
                {
                    // Configure services
                    var services = new ServiceCollection();
                    ConfigureServices(services, connectionString);
                    var serviceProvider = services.BuildServiceProvider();

                    // Create and run main form
                    var dbContext = serviceProvider.GetRequiredService<InfinityPOSDbContext>();
                    var statisticsService = serviceProvider.GetRequiredService<SmartInventoryPro.Services.StatisticsService>();
                    var mainForm = new MainForm(dbContext, statisticsService);
                    Application.Run(mainForm);
                }
            }
            catch (TypeInitializationException ex)
            {
                MessageBox.Show($"خطأ في تهيئة النظام: {ex.InnerException?.Message ?? ex.Message}\n\nتأكد من توفر SQL Server أو قاعدة البيانات", 
                    "خطأ في التهيئة", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في بدء التطبيق: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            MessageBox.Show($"خطأ في التطبيق: {e.Exception.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = (Exception)e.ExceptionObject;
            MessageBox.Show($"خطأ غير متوقع: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private static void ConfigureServices(IServiceCollection services, string connectionString)
        {
            // Add configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
            services.AddSingleton<IConfiguration>(configuration);

            // Add logging
            services.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Information);
            });

            // Add DbContext
            services.AddDbContext<InfinityPOSDbContext>(options =>
                options.UseSqlServer(connectionString));

            // Add Services
            services.AddScoped<SmartInventoryPro.Services.InventoryAnalysisService>();
            services.AddScoped<SmartInventoryPro.Services.StatisticsService>();
        }

        private static DatabaseSettings? LoadSavedDatabaseSettings()
        {
            try
            {
                var settingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SmartInventoryPro", "database_settings.json");
                if (File.Exists(settingsPath))
                {
                    var json = File.ReadAllText(settingsPath);
                    return JsonSerializer.Deserialize<DatabaseSettings>(json);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"خطأ في تحميل الإعدادات المحفوظة: {ex.Message}");
            }
            return null;
        }

        private static string BuildConnectionString(DatabaseSettings settings)
        {
            return $"Server={settings.Server};Database={settings.Database};User Id={settings.Username};Password={settings.Password};TrustServerCertificate=true;MultipleActiveResultSets=true;Encrypt=false;ConnectRetryCount=3;ConnectRetryInterval=10";
        }

        private static bool TestConnection(string connectionString)
        {
            try
            {
                using var connection = new Microsoft.Data.SqlClient.SqlConnection(connectionString);
                connection.Open();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}