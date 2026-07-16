using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    [Table("Disabilities")]
    public class Disability : LookupEntity, IOrderedLookupEntity
    {
        // DfE CBDS DisabilityType code (e.g. MOB, HEAR, VIS) — for statutory/return mapping.
        [StringLength(10)]
        public string? Code { get; set; }

        public int DisplayOrder { get; set; }
    }
}
