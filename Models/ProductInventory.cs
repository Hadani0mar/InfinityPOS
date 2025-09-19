using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InfinityPOS.Models
{
    [Table("Data_ProductInventories", Schema = "Inventory")]
    public class ProductInventory
    {
        [Key]
        [Column("ProductInventoryID_PK")]
        public int ProductInventoryId { get; set; }

        [Column("ProductID_FK")]
        public long ProductId { get; set; }

        [Column("BranchID_FK")]
        public int? BranchId { get; set; }

        [Column("StockOnHand", TypeName = "decimal(18,2)")]
        public decimal? CurrentStockLevel { get; set; }

        [Column("StockOnHold", TypeName = "decimal(18,2)")]
        public decimal? StockOnHold { get; set; }

        [Column("ExpiryDate")]
        public DateTime? ExpiryDate { get; set; }

        [Column("Createddate")]
        public DateTime? LastUpdated { get; set; }

        // Navigation properties
        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }
    }
}
