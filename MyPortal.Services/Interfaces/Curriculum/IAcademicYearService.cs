using MyPortal.Contracts.Models.Curriculum;

namespace MyPortal.Services.Interfaces.Curriculum;

public interface IAcademicYearService
{
    Task<Guid> CreateAcademicYear(AcademicYearUpsertRequest model, CancellationToken cancellationToken);
    Task<Guid> UpdateAcademicYear(Guid academicYearId, AcademicYearUpsertRequest model,
        CancellationToken cancellationToken);
    Task DeleteAcademicYear(Guid academicYearId, CancellationToken cancellationToken);
}