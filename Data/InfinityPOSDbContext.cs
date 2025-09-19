using Microsoft.EntityFrameworkCore;
using InfinityPOS.Models;

namespace InfinityPOS.Data
{
    public class InfinityPOSDbContext : DbContext
    {
        public InfinityPOSDbContext(DbContextOptions<InfinityPOSDbContext> options) : base(options)
        {
        }

        // Define DbSets for all models
        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<ProductGroup> ProductGroups { get; set; } = null!;
        public DbSet<ProductTrademark> ProductTrademarks { get; set; } = null!;
        public DbSet<ProductUom> ProductUoms { get; set; } = null!;
        public DbSet<ProductInventory> ProductInventories { get; set; } = null!;
        public DbSet<ProductExpiryDate> ProductExpiryDates { get; set; } = null!;
        public DbSet<SalesInvoice> SalesInvoices { get; set; } = null!;
        public DbSet<SalesInvoiceItem> SalesInvoiceItems { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Product entity
            modelBuilder.Entity<Product>(entity =>
            {
                entity.ToTable("Data_Products", "Inventory");
                entity.HasKey(e => e.ProductId);
                entity.Property(e => e.ProductCode).HasMaxLength(30);
                entity.Property(e => e.ProductDescription).HasMaxLength(75);
            });

            // Configure ProductGroup entity
            modelBuilder.Entity<ProductGroup>(entity =>
            {
                entity.ToTable("RefProductGroups", "Inventory");
                entity.HasKey(e => e.ProductGroupId);
                entity.Property(e => e.ProductGroupId).HasColumnName("ProductGroupID_PK");
                entity.Property(e => e.ProductGroupDescription).HasMaxLength(255);
            });

            // Configure ProductTrademark entity
            modelBuilder.Entity<ProductTrademark>(entity =>
            {
                entity.ToTable("RefProductTrademarks", "Inventory");
                entity.HasKey(e => e.ProductTrademarkId);
                entity.Property(e => e.ProductTrademarkId).HasColumnName("ProductTrademarkID_PK");
                entity.Property(e => e.ProductTrademarkDescription).HasColumnName("ProductTrademarkDescrption").HasMaxLength(150);
            });

            // Configure ProductUom entity
            modelBuilder.Entity<ProductUom>(entity =>
            {
                entity.ToTable("RefProductUOMs", "Inventory");
                entity.HasKey(e => e.ProductUOMId);
                entity.Property(e => e.ProductUOMDescription).HasMaxLength(100);
            });

            // Configure ProductInventory entity
            modelBuilder.Entity<ProductInventory>(entity =>
            {
                entity.ToTable("Data_ProductInventories", "Inventory");
                entity.HasKey(e => e.ProductInventoryId);
                entity.Property(e => e.CurrentStockLevel).HasColumnType("decimal(18,2)");
                entity.Property(e => e.StockOnHold).HasColumnType("decimal(18,2)");
            });

            // Configure ProductExpiryDate entity
            modelBuilder.Entity<ProductExpiryDate>(entity =>
            {
                entity.ToTable("Data_ProductExpiryDates", "Inventory");
                entity.HasKey(e => e.ExpiryDateId);
                entity.Property(e => e.Quantity).HasColumnType("decimal(18,2)");
            });

            // Configure SalesInvoice entity
            modelBuilder.Entity<SalesInvoice>(entity =>
            {
                entity.ToTable("Data_SalesInvoices", "SALES");
                entity.HasKey(e => e.SalesInvoiceId);
                entity.Property(e => e.InvoiceNumber).HasMaxLength(20);
                entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.CashAmount).HasColumnType("decimal(18,2)");
            });

            // Configure SalesInvoiceItem entity
            modelBuilder.Entity<SalesInvoiceItem>(entity =>
            {
                entity.ToTable("Data_SalesInvoiceItems", "SALES");
                entity.HasKey(e => e.SalesInvoiceItemId);
                entity.Property(e => e.Quantity).HasColumnType("decimal(18,2)");
                entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TotalPrice).HasColumnType("decimal(18,2)");
            });
        }
    }
}