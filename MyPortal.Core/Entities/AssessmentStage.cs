using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    // Statutory assessment stage, e.g. EYFS / end of KS1-5 (CBDS CS064).
    [Table("AssessmentStages")]
    public class AssessmentStage : LookupEntity, IOrderedLookupEntity
    {
        [StringLength(10)]
        public string? Code { get; set; }

        public int DisplayOrder { get; set; }
    }
}
