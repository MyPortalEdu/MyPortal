using MyPortal.Contracts.Models.People;

namespace MyPortal.Services.Interfaces.People;

/// <summary>
/// The welfare / safeguarding area of the student profile — the looked-after care episodes, Personal
/// Education Plans (each with its contributors), child protection plans and the flat welfare indicators
/// held on the Student. The most sharply gated area; enforced at the controller
/// (Student.{View|Edit}StudentWelfare).
/// </summary>
public interface IStudentWelfareService
{
    /// <summary>Welfare area read — the flat indicator FKs, the three child collections and the picker
    /// option lists. 404 if the student doesn't exist.</summary>
    Task<StudentWelfareDetailsResponse> GetWelfareDetailsAsync(Guid studentId, CancellationToken cancellationToken);

    /// <summary>Load-modify-save the four flat welfare indicator FKs on the Student, preserving all other
    /// Student fields.</summary>
    Task UpdateWelfareIndicatorsAsync(Guid studentId, WelfareIndicatorsUpsertRequest model,
        CancellationToken cancellationToken);

    /// <summary>Reconcile the student's care episodes to the supplied set.</summary>
    Task UpdateCareEpisodesAsync(Guid studentId, IEnumerable<CareEpisodeUpsertRequest> model,
        CancellationToken cancellationToken);

    /// <summary>Reconcile the student's Personal Education Plans to the supplied set, and for each PEP
    /// reconcile its contributor set.</summary>
    Task UpdatePepsAsync(Guid studentId, IEnumerable<PepUpsertRequest> model, CancellationToken cancellationToken);

    /// <summary>Reconcile the student's child protection plans to the supplied set.</summary>
    Task UpdateChildProtectionPlansAsync(Guid studentId, IEnumerable<ChildProtectionPlanUpsertRequest> model,
        CancellationToken cancellationToken);
}
