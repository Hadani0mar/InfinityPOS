using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InfinityPOS.Models
{
    [Table("RefProductGroups", Schema = "Inventory")]
    public class ProductGroup
    {
        [Key]
        [Column("ProductGroupID_PK")]
        public int ProductGroupId { get; set; }

        [Column("ProductGroupDescription")]
        [StringLength(255)]
        public string? ProductGroupDescription { get; set; }

        [Column("IsSynced")]
        public bool IsSynced { get; set; } = false;

        // Navigation properties
        public virtual ICollection<Product>? Products { get; set; }
    }
}
