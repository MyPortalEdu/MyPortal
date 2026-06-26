using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    // Body that reviewed an exclusion - governing board or independent review panel (CBDS CS137).
    [Table("ExclusionReviewCategories")]
    public class ExclusionReviewCategory : LookupEntity, IOrderedLookupEntity
    {
        [StringLength(10)]
        public string? Code { get; set; }

        public int DisplayOrder { get; set; }
    }
}
