using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartInventoryPro.Models
{
    [Table("RefUOMs", Schema = "Inventory")]
    public class ProductUom
    {
        [Key]
        [Column("UOMID_PK")]
        public short ProductUOMId { get; set; }

        [Column("UOMName")]
        [StringLength(100)]
        public string? ProductUOMDescription { get; set; }

        [Column("IsSynced")]
        public bool IsSynced { get; set; } = false;

        // Navigation properties
        public virtual ICollection<Product>? Products { get; set; }
    }
}
