namespace MyPortal.Contracts.Models.People;

/// <summary>
/// Read payload for the welfare / safeguarding area of the student profile — the looked-after care
/// episodes, Personal Education Plans, child protection plans and the flat welfare indicators held on
/// the Student, with the active option lists so the editor is self-contained. The most sharply gated
/// area (Student.ViewStudentWelfare).
/// </summary>
public class StudentWelfareDetailsResponse
{
    public Guid StudentId { get; set; }

    public Guid? PostLookedAfterArrangementId { get; set; }
    public Guid? ServiceChildIndicatorId { get; set; }
    public Guid? YoungCarerIndicatorId { get; set; }
    public Guid? KinshipCareIndicatorId { get; set; }

    public List<CareEpisodeResponse> CareEpisodes { get; set; } = [];
    public List<PepResponse> Peps { get; set; } = [];
    public List<ChildProtectionPlanResponse> ChildProtectionPlans { get; set; } = [];

    public List<LookupResponse> LivingArrangements { get; set; } = [];
    public List<LookupResponse> CaringAuthorities { get; set; } = [];
    public List<LookupResponse> PostLookedAfterArrangements { get; set; } = [];
    public List<LookupResponse> ServiceChildIndicators { get; set; } = [];
    public List<LookupResponse> YoungCarerIndicators { get; set; } = [];
    public List<LookupResponse> KinshipCareIndicators { get; set; } = [];
}
