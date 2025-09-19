using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using SmartInventoryPro.Data;
using SmartInventoryPro.Forms;
using System.Text;

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
                // Show database connection form
                var dbConnectionForm = new DatabaseConnectionForm();
                if (dbConnectionForm.ShowDialog() == DialogResult.OK)
                {
                    var connectionString = dbConnectionForm.ConnectionString;
                    
                    // Configure services
                    var services = new ServiceCollection();
                    ConfigureServices(services, connectionString!);
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
    }
}