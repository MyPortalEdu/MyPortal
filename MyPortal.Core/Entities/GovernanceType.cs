using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("GovernanceTypes")]
    public class GovernanceType : LookupEntity
    {
        [Required]
        [StringLength(10)]
        public required string Code { get; set; }
    }
}