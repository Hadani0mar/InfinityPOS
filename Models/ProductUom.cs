using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InfinityPOS.Models
{
    [Table("RefProductUOMs", Schema = "Inventory")]
    public class ProductUom
    {
        [Key]
        [Column("ProductUOMId")]
        public int ProductUOMId { get; set; }

        [Column("ProductUOMDescription")]
        [StringLength(100)]
        public string? ProductUOMDescription { get; set; }

        [Column("IsSynced")]
        public bool IsSynced { get; set; } = false;

        // Navigation properties
        public virtual ICollection<Product>? Products { get; set; }
    }
}
