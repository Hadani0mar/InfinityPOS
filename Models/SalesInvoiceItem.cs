using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartInventoryPro.Models
{
    [Table("Data_SalesInvoiceItems", Schema = "SALES")]
    public class SalesInvoiceItem
    {
        [Key]
        [Column("SalesInvoiceItemID_PK")]
        public long SalesInvoiceItemId { get; set; }

        [Column("SalesInvoiceID_FK")]
        public int SalesInvoiceId { get; set; }

        [Column("ProductID_FK")]
        public long ProductId { get; set; }

        [Column("QYT", TypeName = "decimal(18,2)")]
        public decimal? Quantity { get; set; }

        [Column("UnitPrice", TypeName = "decimal(18,2)")]
        public decimal? UnitPrice { get; set; }

        [Column("SubTotal", TypeName = "decimal(18,2)")]
        public decimal? TotalPrice { get; set; }

        // Navigation properties
        [ForeignKey("SalesInvoiceId")]
        public virtual SalesInvoice? SalesInvoice { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }
    }
}
