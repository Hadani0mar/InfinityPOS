using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InfinityPOS.Models
{
    [Table("Data_Products", Schema = "Inventory")]
    public class Product
    {
        [Key]
        [Column("ProductID_PK")]
        public long ProductId { get; set; }

        [Column("ProductCode")]
        [StringLength(50)]
        public string? ProductCode { get; set; }

        [Column("ProductName")]
        [StringLength(75)]
        public string? ProductDescription { get; set; }

        [Column("ProductGroupID_FK")]
        public int? ProductGroupId { get; set; }

        [Column("ProductTrademarkID_FK")]
        public int? ProductTrademarkId { get; set; }

        [Column("DefaultSellUomID_FK")]
        public int? ProductUOMId { get; set; }

        [Column("IsInActive")]
        public bool IsInActive { get; set; } = false;

        [Column("CreatedDate")]
        public DateTime? CreatedDate { get; set; }

        [Column("ModifiedDate")]
        public DateTime? ModifiedDate { get; set; }

        [Column("CreatedByUserId")]
        public int? CreatedByUserId { get; set; }

        [Column("CreatedByUserName")]
        [StringLength(100)]
        public string? CreatedByUserName { get; set; }

        [Column("ModifiedByUserId")]
        public int? ModifiedByUserId { get; set; }

        [Column("ModifiedByUserName")]
        [StringLength(100)]
        public string? ModifiedByUserName { get; set; }

        // Navigation properties
        [ForeignKey("ProductGroupId")]
        public virtual ProductGroup? ProductGroup { get; set; }

        [ForeignKey("ProductTrademarkId")]
        public virtual ProductTrademark? ProductTrademark { get; set; }

        [ForeignKey("ProductUOMId")]
        public virtual ProductUom? ProductUom { get; set; }

        public virtual ICollection<ProductInventory>? ProductInventories { get; set; }
        public virtual ICollection<ProductExpiryDate>? ProductExpiryDates { get; set; }
        public virtual ICollection<SalesInvoiceItem>? SalesInvoiceItems { get; set; }
    }
}
