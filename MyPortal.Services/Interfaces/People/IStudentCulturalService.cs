using MyPortal.Contracts.Models.People;
using MyPortal.Contracts.Models.People.Students;

namespace MyPortal.Services.Interfaces.People;

/// <summary>
/// The Cultural area of the student profile — the pupil's ethnic/cultural demographics (on the shared
/// Person) and English proficiency (on the Student). GDPR special-category; gated on Student.Cultural.
/// </summary>
public interface IStudentCulturalService
{
    /// <summary>Cultural area read — ethnicity/language/religion/nationality + English proficiency,
    /// with the picker option lists. 404 if the student doesn't exist.</summary>
    Task<StudentCulturalDetailsResponse> GetCulturalDetailsAsync(Guid studentId, CancellationToken cancellationToken);

    /// <summary>Cultural area write — the pupil's ethnic/cultural fields only; other person fields are
    /// left untouched.</summary>
    Task UpdateCulturalDetailsAsync(Guid studentId, StudentCulturalDetailsUpsertRequest model,
        CancellationToken cancellationToken);
}
