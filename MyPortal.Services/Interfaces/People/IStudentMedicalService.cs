using MyPortal.Contracts.Models.People;

namespace MyPortal.Services.Interfaces.People;

/// <summary>
/// The Medical area of the student profile — the medical-needs flag (on the shared Person) plus the
/// pupil's medical conditions, dietary requirements and disabilities. GDPR special-category; gated on
/// Student.Medical.
/// </summary>
public interface IStudentMedicalService
{
    /// <summary>Medical area read — the medical-needs flag and the three linked collections, with the
    /// picker option lists. 404 if the student doesn't exist.</summary>
    Task<StudentMedicalDetailsResponse> GetMedicalDetailsAsync(Guid studentId, CancellationToken cancellationToken);

    /// <summary>Medical area write — the medical-needs flag plus a reconcile of the pupil's conditions,
    /// dietary requirements and disabilities; other person fields are left untouched.</summary>
    Task UpdateMedicalDetailsAsync(Guid studentId, StudentMedicalDetailsUpsertRequest model,
        CancellationToken cancellationToken);
}
