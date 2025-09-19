using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InfinityPOS.Models
{
    [Table("RefProductTrademarks", Schema = "Inventory")]
    public class ProductTrademark
    {
        [Key]
        [Column("ProductTrademarkID_PK")]
        public int ProductTrademarkId { get; set; }

        [Column("ProductTrademarkDescrption")] // اسم العمود الصحيح من قاعدة البيانات
        [StringLength(150)]
        public string? ProductTrademarkDescription { get; set; }

        [Column("IsSynced")]
        public bool IsSynced { get; set; } = false;

        // Navigation properties
        public virtual ICollection<Product>? Products { get; set; }
    }
}
