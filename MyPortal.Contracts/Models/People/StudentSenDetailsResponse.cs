namespace MyPortal.Contracts.Models.People;

/// <summary>
/// Read payload for the SEN area of the student profile — the cached current SEN status (denormalised
/// on the Student) plus the dated status history, ranked needs, provisions and statutory statements,
/// with the active option lists so the editor is self-contained. Gated on Student.ViewStudentSen.
/// </summary>
public class StudentSenDetailsResponse
{
    public Guid StudentId { get; set; }

    public Guid? CurrentSenStatusId { get; set; }
    public DateTime? SenStartDate { get; set; }
    public bool SenUnitMember { get; set; }
    public bool ResourcedProvisionMember { get; set; }

    public List<SenStatusHistoryResponse> StatusHistory { get; set; } = [];
    public List<SenNeedResponse> Needs { get; set; } = [];
    public List<SenProvisionResponse> Provisions { get; set; } = [];
    public List<SenStatementResponse> Statements { get; set; } = [];

    public List<LookupResponse> SenStatuses { get; set; } = [];
    public List<LookupResponse> SenTypes { get; set; } = [];
    public List<LookupResponse> SenProvisionTypes { get; set; } = [];
    public List<LookupResponse> StatutoryAssessmentAgreedOptions { get; set; } = [];
    public List<LookupResponse> StatutoryAssessmentOutcomeOptions { get; set; } = [];
}
