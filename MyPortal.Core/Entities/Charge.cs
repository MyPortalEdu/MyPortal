using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("Charges")]
    public class Charge : LookupEntity
    {
        public Guid VatRateId { get; set; }

        [Required]
        [StringLength(64)]
        public required string Code { get; set; }
        
        [Required]
        [StringLength(128)]
        public required string Name { get; set; }
        
        public decimal Amount { get; set; }

        public bool IsVariable { get; set; }

        public VatRate? VatRate { get; set; }
    }
}