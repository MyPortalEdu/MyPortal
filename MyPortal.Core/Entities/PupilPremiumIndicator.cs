using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    // Which pupil premium a pupil attracts, e.g. deprivation / service / post-LAC (CBDS CS106).
    [Table("PupilPremiumIndicators")]
    public class PupilPremiumIndicator : LookupEntity, IOrderedLookupEntity
    {
        [StringLength(10)]
        public string? Code { get; set; }

        public int DisplayOrder { get; set; }
    }
}
