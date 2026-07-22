using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    // Whether the local authority agreed to carry out a statutory (EHC) assessment.
    [Table("SenStatutoryAssessmentAgreed")]
    public class SenStatutoryAssessmentAgreed : LookupEntity
    {
    }
}
