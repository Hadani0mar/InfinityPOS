using System;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace InfinityPOS.Services
{
    public class StatisticsService
    {
        private readonly string _connectionString;

        public StatisticsService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        public async Task<DashboardStatistics> GetDashboardStatisticsAsync()
        {
            var stats = new DashboardStatistics();

            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                // تنفيذ الاستعلامات المختلفة
                await GetExpiryStatisticsAsync(connection, stats);
                await GetTopSellingProductAsync(connection, stats);
                await GetTopSellingProductGroupAsync(connection, stats);
                await GetStockStatisticsAsync(connection, stats);
                await GetSalesStatisticsAsync(connection, stats);
                await GetCashStatisticsAsync(connection, stats);

                return stats;
            }
            catch (Exception ex)
            {
                throw new Exception($"خطأ في جلب الإحصائيات: {ex.Message}");
            }
        }

        private async Task GetExpiryStatisticsAsync(SqlConnection connection, DashboardStatistics stats)
        {
            var query = @"
                DECLARE @today date = CAST(GETDATE() AS date);
                
                WITH inv AS (
                    SELECT p.ProductID_PK AS ProductID, 
                           ISNULL(pi.ExpiryDate, ped.ExpiryDate) AS ExpiryDate,
                           COALESCE(pi.StockOnHand, 0) AS StockOnHand
                    FROM Inventory.Data_Products p
                    LEFT JOIN Inventory.Data_ProductInventories pi ON pi.ProductID_FK = p.ProductID_PK
                    LEFT JOIN Inventory.Data_ProductExpiryDates ped ON ped.ProductID_FK = p.ProductID_PK
                )
                SELECT
                    SUM(CASE WHEN i.ExpiryDate < @today AND i.StockOnHand > 0 THEN 1 ELSE 0 END) AS ExpiredCount,
                    SUM(CASE WHEN i.ExpiryDate >= @today AND i.ExpiryDate < DATEADD(DAY,30,@today) AND i.StockOnHand > 0 THEN 1 ELSE 0 END) AS NearExpiryCount
                FROM (
                    SELECT ProductID, MIN(ExpiryDate) AS ExpiryDate, SUM(StockOnHand) AS StockOnHand
                    FROM inv
                    WHERE ExpiryDate IS NOT NULL
                    GROUP BY ProductID
                ) i;";

            using var command = new SqlCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();
            
            if (await reader.ReadAsync())
            {
                stats.ExpiredProductsCount = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                stats.NearExpiryProductsCount = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);
            }
        }

        private async Task GetTopSellingProductAsync(SqlConnection connection, DashboardStatistics stats)
        {
            var query = @"
                DECLARE @today date = CAST(GETDATE() AS date);
                DECLARE @threeMonthsAgo date = DATEADD(MONTH, -3, @today);

                WITH sales_m AS (
                    SELECT sii.ProductID_FK AS ProductID, SUM(ISNULL(sii.UnitBaseQYT, sii.QYT)) AS Qty
                    FROM SALES.Data_SalesInvoiceItems sii
                    INNER JOIN SALES.Data_SalesInvoices si ON si.SalesInvoiceID_PK = sii.SalesInvoiceID_FK
                    WHERE si.SalesInvoiceDate >= @threeMonthsAgo
                    GROUP BY sii.ProductID_FK
                )
                SELECT TOP 1 p.ProductID_PK AS ProductID, p.ProductName, s.Qty
                FROM sales_m s
                JOIN Inventory.Data_Products p ON p.ProductID_PK = s.ProductID
                ORDER BY s.Qty DESC;";

            using var command = new SqlCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();
            
            if (await reader.ReadAsync())
            {
                stats.TopSellingProduct = reader.IsDBNull(1) ? "غير محدد" : reader.GetString(1);
                stats.TopSellingProductQuantity = reader.IsDBNull(2) ? 0 : reader.GetDecimal(2);
            }
        }

        private async Task GetTopSellingProductGroupAsync(SqlConnection connection, DashboardStatistics stats)
        {
            var query = @"
                DECLARE @today date = CAST(GETDATE() AS date);
                DECLARE @threeMonthsAgo date = DATEADD(MONTH, -3, @today);

                WITH sales_g AS (
                    SELECT p.ProductGroupID_FK AS GroupID, SUM(ISNULL(sii.UnitBaseQYT, sii.QYT)) AS Qty
                    FROM SALES.Data_SalesInvoiceItems sii
                    INNER JOIN SALES.Data_SalesInvoices si ON si.SalesInvoiceID_PK = sii.SalesInvoiceID_FK
                    INNER JOIN Inventory.Data_Products p ON p.ProductID_PK = sii.ProductID_FK
                    WHERE si.SalesInvoiceDate >= @threeMonthsAgo
                    GROUP BY p.ProductGroupID_FK
                )
                SELECT TOP 1 s.GroupID, rg.ProductGroupDescription, s.Qty
                FROM sales_g s
                JOIN Inventory.RefProductGroups rg ON rg.ProductGroupID_PK = s.GroupID
                ORDER BY s.Qty DESC;";

            using var command = new SqlCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();
            
            if (await reader.ReadAsync())
            {
                stats.TopSellingProductGroup = reader.IsDBNull(1) ? "غير محدد" : reader.GetString(1);
                stats.TopSellingGroupQuantity = reader.IsDBNull(2) ? 0 : reader.GetDecimal(2);
            }
        }

        private async Task GetStockStatisticsAsync(SqlConnection connection, DashboardStatistics stats)
        {
            var query = @"
                SELECT
                    COUNT(*) AS TotalProducts,
                    SUM(CASE WHEN p.StockOnHand <= ISNULL(NULLIF(p.MinStockLevel,0), 10) AND p.StockOnHand > 0 THEN 1 ELSE 0 END) AS NearDepletionCount,
                    SUM(CASE WHEN p.StockOnHand <= 0 THEN 1 ELSE 0 END) AS OutOfStockCount
                FROM Inventory.Data_Products p
                WHERE p.IsInActive = 0;"; // IsInActive = 0 means active

            using var command = new SqlCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();
            
            if (await reader.ReadAsync())
            {
                stats.TotalProducts = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                stats.LowStockCount = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);
                stats.OutOfStockCount = reader.IsDBNull(2) ? 0 : reader.GetInt32(2);
            }
        }

        private async Task GetSalesStatisticsAsync(SqlConnection connection, DashboardStatistics stats)
        {
            var query = @"
                -- إحصائيات آخر 3 أشهر
                DECLARE @today date = CAST(GETDATE() AS date);
                DECLARE @threeMonthsAgo date = DATEADD(MONTH, -3, @today);

                SELECT 
                    ISNULL(SUM(si.InvoiceNetTotal), 0) AS TotalSalesThisMonth,
                    COUNT(si.SalesInvoiceID_PK) AS TotalInvoicesThisMonth
                FROM SALES.Data_SalesInvoices si
                WHERE si.SalesInvoiceDate >= @threeMonthsAgo;";

            using var command = new SqlCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();
            
            if (await reader.ReadAsync())
            {
                stats.TotalSalesThisMonth = reader.IsDBNull(0) ? 0 : reader.GetDecimal(0);
                stats.TotalInvoicesThisMonth = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);
            }
        }

        private async Task GetCashStatisticsAsync(SqlConnection connection, DashboardStatistics stats)
        {
            var query = @"
                -- إجمالي النقد لآخر 3 أشهر
                DECLARE @today date = CAST(GETDATE() AS date);
                DECLARE @threeMonthsAgo date = DATEADD(MONTH, -3, @today);

                SELECT SUM(ISNULL(Cash, 0)) AS TotalCash
                FROM SALES.Data_SalesInvoices
                WHERE SalesInvoiceDate >= @threeMonthsAgo;";

            using var command = new SqlCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();
            
            if (await reader.ReadAsync())
            {
                stats.TotalCashThisMonth = reader.IsDBNull(0) ? 0 : reader.GetDecimal(0);
            }
        }
    }

    public class DashboardStatistics
    {
        public int TotalProducts { get; set; }
        public int LowStockCount { get; set; }
        public int OutOfStockCount { get; set; }
        public int ExpiredProductsCount { get; set; }
        public int NearExpiryProductsCount { get; set; }
        public decimal TotalSalesThisMonth { get; set; }
        public decimal TotalCashThisMonth { get; set; }
        public int TotalInvoicesThisMonth { get; set; }
        public string TopSellingProduct { get; set; } = "غير محدد";
        public decimal TopSellingProductQuantity { get; set; }
        public string TopSellingProductGroup { get; set; } = "غير محدد";
        public decimal TopSellingGroupQuantity { get; set; }
    }
}
