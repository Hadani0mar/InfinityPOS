using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using InfinityPOS.Data;
using InfinityPOS.Models;

namespace InfinityPOS.Services
{
    public class InventoryAnalysisService
    {
        private readonly InfinityPOSDbContext _dbContext;

        public InventoryAnalysisService(InfinityPOSDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// تحليل الأصناف المطلوبة بناءً على خوارزمية ذكية
        /// </summary>
        public async Task<List<RequiredItemResult>> GetRequiredItemsAsync()
        {
            var requiredItems = new List<RequiredItemResult>();

            try
            {
                // جلب المنتجات مع مخزونها الحالي
                var products = await _dbContext.Products
                    .Include(p => p.ProductInventories)
                        .Where(p => !p.IsInActive) // IsInActive = false means active
                    .ToListAsync();

                foreach (var product in products)
                {
                    var inventory = product.ProductInventories?.FirstOrDefault();
                    if (inventory == null) continue;

                    var currentStock = inventory.CurrentStockLevel ?? 0;
                    // استخدام قيم افتراضية للحد الأدنى والأقصى من جدول المنتجات
                    var minStock = 10m; // قيمة افتراضية
                    var maxStock = 100m; // قيمة افتراضية

                    // حساب معدل الاستهلاك اليومي
                    var dailyConsumption = await CalculateDailyConsumptionAsync(product.ProductId);
                    
                    // حساب أيام التغطية المتبقية
                    var daysCoverage = dailyConsumption > 0 ? (double)(currentStock / dailyConsumption) : 999;
                    
                    // تحديد الأولوية
                    var priority = CalculatePriority(currentStock, minStock, daysCoverage, dailyConsumption);
                    
                    // الكمية المقترحة للطلب
                    var suggestedQuantity = CalculateSuggestedQuantity(currentStock, minStock, maxStock, dailyConsumption);

                    if (priority > 0) // إذا كان المنتج يحتاج إعادة طلب
                    {
                        requiredItems.Add(new RequiredItemResult
                        {
                            ProductId = product.ProductId,
                            ProductCode = product.ProductCode,
                            ProductName = product.ProductDescription,
                            CurrentStock = currentStock,
                            MinimumStock = minStock,
                            DailyConsumption = dailyConsumption,
                            DaysCoverage = Math.Round(daysCoverage, 1),
                            Priority = priority,
                            SuggestedQuantity = suggestedQuantity,
                            Status = GetStockStatus(currentStock, minStock, daysCoverage)
                        });
                    }
                }

                return requiredItems.OrderByDescending(x => x.Priority).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"خطأ في تحليل الأصناف المطلوبة: {ex.Message}");
            }
        }

        /// <summary>
        /// حساب معدل الاستهلاك اليومي للمنتج
        /// </summary>
        private async Task<decimal> CalculateDailyConsumptionAsync(long productId)
        {
            try
            {
                var thirtyDaysAgo = DateTime.Now.AddDays(-30);
                
                var salesData = await _dbContext.SalesInvoiceItems
                    .Include(sii => sii.SalesInvoice)
                    .Where(sii => sii.ProductId == productId && 
                                  sii.SalesInvoice!.InvoiceDate >= thirtyDaysAgo)
                    .SumAsync(sii => sii.Quantity ?? 0);

                return salesData / 30; // متوسط يومي
            }
            catch
            {
                return 1; // قيمة افتراضية
            }
        }

        /// <summary>
        /// حساب أولوية الطلب
        /// </summary>
        private int CalculatePriority(decimal currentStock, decimal minStock, double daysCoverage, decimal dailyConsumption)
        {
            var priority = 0;

            // أولوية عالية جداً - مخزون منتهي
            if (currentStock <= 0) priority += 100;
            
            // أولوية عالية - أقل من الحد الأدنى
            else if (currentStock <= minStock) priority += 80;
            
            // أولوية متوسطة - قريب من الحد الأدنى
            else if (currentStock <= minStock * 1.2m) priority += 60;

            // إضافة أولوية بناءً على أيام التغطية
            if (daysCoverage <= 3) priority += 50;
            else if (daysCoverage <= 7) priority += 30;
            else if (daysCoverage <= 14) priority += 15;

            // إضافة أولوية بناءً على معدل الاستهلاك
            if (dailyConsumption > 10) priority += 20; // منتج عالي الحركة
            else if (dailyConsumption > 5) priority += 10;

            return priority;
        }

        /// <summary>
        /// حساب الكمية المقترحة للطلب
        /// </summary>
        private decimal CalculateSuggestedQuantity(decimal currentStock, decimal minStock, decimal maxStock, decimal dailyConsumption)
        {
            // الكمية المطلوبة للوصول للحد الأقصى
            var targetQuantity = maxStock - currentStock;
            
            // إضافة مخزون أمان لـ 30 يوم
            var safetyStock = dailyConsumption * 30;
            
            return Math.Max(targetQuantity, safetyStock);
        }

        /// <summary>
        /// تحديد حالة المخزون
        /// </summary>
        private string GetStockStatus(decimal currentStock, decimal minStock, double daysCoverage)
        {
            if (currentStock <= 0) return "منتهي";
            if (currentStock <= minStock) return "حرج";
            if (daysCoverage <= 7) return "منخفض";
            if (daysCoverage <= 14) return "متوسط";
            return "جيد";
        }

        /// <summary>
        /// تحليل أداء الموظفين
        /// </summary>
        public async Task<List<EmployeePerformanceResult>> AnalyzeEmployeePerformanceAsync()
        {
            var results = new List<EmployeePerformanceResult>();

            try
            {
                var thirtyDaysAgo = DateTime.Now.AddDays(-30);
                
                var salesByEmployee = await _dbContext.SalesInvoices
                    .Where(si => si.InvoiceDate >= thirtyDaysAgo)
                    .GroupBy(si => si.CreatedByUserId)
                    .Select(g => new
                    {
                        EmployeeId = g.Key,
                        TotalSales = g.Sum(si => si.TotalAmount ?? 0),
                        TransactionCount = g.Count(),
                        AverageTransactionValue = g.Average(si => si.TotalAmount ?? 0)
                    })
                    .ToListAsync();

                foreach (var emp in salesByEmployee)
                {
                    if (emp.EmployeeId.HasValue)
                    {
                        results.Add(new EmployeePerformanceResult
                        {
                            EmployeeId = emp.EmployeeId.Value,
                            EmployeeName = $"موظف {emp.EmployeeId}", // يمكن تحسينه لجلب الاسم الفعلي
                            TotalSales = emp.TotalSales,
                            TransactionCount = emp.TransactionCount,
                            AverageTransactionValue = emp.AverageTransactionValue,
                            PerformanceScore = CalculatePerformanceScore(emp.TotalSales, emp.TransactionCount, emp.AverageTransactionValue)
                        });
                    }
                }

                return results.OrderByDescending(x => x.PerformanceScore).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"خطأ في تحليل أداء الموظفين: {ex.Message}");
            }
        }

        private double CalculatePerformanceScore(decimal totalSales, int transactionCount, decimal avgTransactionValue)
        {
            var score = 0.0;
            
            // 40% من النتيجة للمبيعات الإجمالية
            score += (double)(totalSales / 10000) * 40;
            
            // 30% لعدد المعاملات
            score += (transactionCount / 100.0) * 30;
            
            // 30% لمتوسط قيمة المعاملة
            score += (double)(avgTransactionValue / 500) * 30;
            
            return Math.Min(score, 100); // الحد الأقصى 100
        }
    }

    // نماذج النتائج
    public class RequiredItemResult
    {
        public long ProductId { get; set; }
        public string? ProductCode { get; set; }
        public string? ProductName { get; set; }
        public decimal CurrentStock { get; set; }
        public decimal MinimumStock { get; set; }
        public decimal DailyConsumption { get; set; }
        public double DaysCoverage { get; set; }
        public int Priority { get; set; }
        public decimal SuggestedQuantity { get; set; }
        public string? Status { get; set; }
    }

    public class EmployeePerformanceResult
    {
        public int EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public decimal TotalSales { get; set; }
        public int TransactionCount { get; set; }
        public decimal AverageTransactionValue { get; set; }
        public double PerformanceScore { get; set; }
    }
}
