using MyPortal.Contracts.Models.Curriculum;

namespace MyPortal.Services.Interfaces.Curriculum;

public interface IAcademicYearService
{
    Task<IList<AcademicYearSummaryResponse>> ListAsync(CancellationToken cancellationToken);

    Task<AcademicYearDetailsResponse> GetByIdAsync(Guid academicYearId, CancellationToken cancellationToken);

    // Returns null if no AY has started yet (every AY's first term is in the future).
    Task<AcademicYearSummaryResponse?> GetCurrentAsync(CancellationToken cancellationToken);

    Task<Guid> CreateAcademicYear(AcademicYearUpsertRequest model, CancellationToken cancellationToken);
    Task<Guid> UpdateAcademicYear(Guid academicYearId, AcademicYearUpsertRequest model,
        CancellationToken cancellationToken);
    Task DeleteAcademicYear(Guid academicYearId, CancellationToken cancellationToken);
}
