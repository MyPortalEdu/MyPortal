using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    // Post-looked-after arrangement, e.g. adopted / SGO / CAO (CBDS CS087).
    [Table("PostLookedAfterArrangements")]
    public class PostLookedAfterArrangement : LookupEntity, IOrderedLookupEntity
    {
        [StringLength(10)]
        public string? Code { get; set; }

        public int DisplayOrder { get; set; }
    }
}
