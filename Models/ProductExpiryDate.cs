using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InfinityPOS.Models
{
    [Table("Data_ProductExpiryDates", Schema = "Inventory")]
    public class ProductExpiryDate
    {
        [Key]
        [Column("ExpiryDateID_PK")]
        public long ExpiryDateId { get; set; }

        [Column("ProductID_FK")]
        public long ProductId { get; set; }

        [Column("ExpiryDate")]
        public DateTime? ExpiryDate { get; set; }

        [Column("QYT", TypeName = "decimal(18,2)")]
        public decimal? Quantity { get; set; }

        [Column("Createddate")]
        public DateTime? CreatedDate { get; set; }

        // Navigation properties
        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }
    }
}
