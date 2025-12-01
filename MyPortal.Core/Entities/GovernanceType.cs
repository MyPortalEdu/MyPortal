using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("GovernanceTypes")]
    public class GovernanceType : LookupEntity
    {
        [Required]
        [StringLength(10)] 
        public string Code { get; set; } = null!;
    }
}