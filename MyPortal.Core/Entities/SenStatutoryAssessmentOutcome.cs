using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    // The outcome of a statutory (EHC) assessment, e.g. plan issued or no plan.
    [Table("SenStatutoryAssessmentOutcome")]
    public class SenStatutoryAssessmentOutcome : LookupEntity
    {
    }
}
