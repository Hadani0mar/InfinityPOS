using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using SmartInventoryPro.Data;
using SmartInventoryPro.Forms;

namespace SmartInventoryPro
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
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
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في بدء التطبيق: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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