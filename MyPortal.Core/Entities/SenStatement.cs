using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    // A statutory SEN plan record: an EHC plan (IsEhcp = true) or a legacy statement. Holds the
    // statutory-assessment lifecycle dates and the associated legal/dispute detail. England-only.
    [Table("SenStatements")]
    public class SenStatement : Entity
    {
        public Guid StudentId { get; set; }

        // EHC plan (true) versus a legacy SEN statement (false).
        public bool IsEhcp { get; set; }

        public DateTime AssessmentRequestDate { get; set; }

        public DateTime? ParentConsultDate { get; set; }

        // Date the plan/statement was finalised (issued).
        public DateTime? FinalisedDate { get; set; }

        public DateTime? CeasedDate { get; set; }

        public Guid? StatutoryAssessmentAgreedId { get; set; }

        public Guid? StatutoryAssessmentOutcomeId { get; set; }

        public bool SubjectToTribunal { get; set; }

        public bool UndergoingMediation { get; set; }

        [StringLength(1024)]
        public string? AppealNotes { get; set; }

        [StringLength(256)]
        public string? TemporaryDisapplicationSubjects { get; set; }

        [StringLength(256)]
        public string? PermanentDisapplicationSubjects { get; set; }

        // The local authority responsible for the plan.
        public Guid? LocalAuthorityId { get; set; }

        [StringLength(1024)]
        public string? Comments { get; set; }

        public Student? Student { get; set; }
        public SenStatutoryAssessmentAgreed? StatutoryAssessmentAgreed { get; set; }
        public SenStatutoryAssessmentOutcome? StatutoryAssessmentOutcome { get; set; }
        public LocalAuthority? LocalAuthority { get; set; }
    }
}
