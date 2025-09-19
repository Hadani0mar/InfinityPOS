using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartInventoryPro.Models
{
    [Table("Data_SalesInvoices", Schema = "SALES")]
    public class SalesInvoice
    {
        [Key]
        [Column("SalesInvoiceID_PK")]
        public int SalesInvoiceId { get; set; }

        [Column("InvoiceNumber")]
        [StringLength(20)]
        public string? InvoiceNumber { get; set; }

        [Column("SalesInvoiceDate")]
        public DateTime? InvoiceDate { get; set; }

        [Column("CustomerID_FK")]
        public int? CustomerId { get; set; }

        [Column("InvoiceNetTotal", TypeName = "decimal(18,2)")]
        public decimal? TotalAmount { get; set; }

        [Column("Cash", TypeName = "decimal(18,2)")]
        public decimal? CashAmount { get; set; }

        [Column("Createddate")]
        public DateTime? CreatedDate { get; set; }

        [Column("CreatedByUserID")]
        public short? CreatedByUserId { get; set; }

        [Column("SalePersonID_FK")]
        public int? SalePersonId { get; set; }

        // Navigation properties
        public virtual ICollection<SalesInvoiceItem>? SalesInvoiceItems { get; set; }
    }
}
