using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("Discounts")]
    public class Discount : LookupEntity
    {
        [Required]
        [StringLength(128)]
        public string Name { get; set; } = null!;
        
        public decimal Amount { get; set; }

        public bool IsPercentage { get; set; }

        // Specify whether this discount can be combined with other discounts
        public bool BlockOtherDiscounts { get; set; }
    }
}