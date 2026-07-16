using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("InductionStatuses")]
    public class InductionStatus : LookupEntity
    {
        [StringLength(9)]
        public string? ColourCode { get; set; }
    }
}
