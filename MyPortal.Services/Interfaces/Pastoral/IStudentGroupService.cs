using System.Data;
using MyPortal.Common.Interfaces;
using MyPortal.Contracts.Models.Pastoral;

namespace MyPortal.Services.Interfaces.Pastoral;

public interface IStudentGroupService
{
    // Inserts a StudentGroup row, its supervisors, and (if one is flagged main)
    // backfills MainSupervisorId. Validates the AY is not locked. Returns the
    // new StudentGroup.Id.
    Task<Guid> CreateAsync(Guid academicYearId, StudentGroupUpsertCore core,
        IList<StudentGroupSupervisorUpsertRequest> supervisors,
        CancellationToken cancellationToken, IUnitOfWork? uow = null);

    // Updates a StudentGroup's core fields and replace-all its supervisors.
    // Validates the AY (looked up from the existing group) is not locked.
    Task UpdateAsync(Guid studentGroupId, StudentGroupUpsertCore core,
        IList<StudentGroupSupervisorUpsertRequest> supervisors,
        CancellationToken cancellationToken, IUnitOfWork? uow = null);

    // Deletes a StudentGroup and its supervisors. Caller MUST have already deleted
    // the IStudentGroupEntity row(s) that hang off this group (House/YearGroup/
    // RegGroup) so the StudentGroup delete doesn't trip the FK. Validates the AY
    // is not locked and the group has no downstream data attached.
    Task DeleteAsync(Guid studentGroupId,
        CancellationToken cancellationToken, IUnitOfWork? uow = null);

    // Throws AcademicYearLockedException if the AY is locked. Exposed so callers
    // can gate non-Create/Update/Delete operations on the same lock semantics.
    Task EnsureAcademicYearNotLockedAsync(Guid academicYearId, string verb,
        CancellationToken cancellationToken, IDbTransaction? transaction = null);
}
