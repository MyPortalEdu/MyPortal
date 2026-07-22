using Microsoft.Extensions.Logging;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Exceptions;
using MyPortal.Contracts.Models.Documents;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Services.Documents;
using MyPortal.Services.Interfaces;
using MyPortal.Services.Interfaces.Documents;
using MyPortal.Services.Interfaces.People;

namespace MyPortal.Services.People.Staff;

/// <summary>
/// Staff-scoped attachments. The route's <c>entityId</c> is the <em>staff member</em> id;
/// <see cref="GetByIdAsync"/> resolves it to the owning <see cref="Person"/> whose root directory
/// holds the files. The structural rules (subtree scoping, private/upload flags) come from the
/// base; the entity-level gate here defers to the staff <c>Documents</c> permission domain via
/// <see cref="IStaffMemberAccessService"/>, so view/edit follow the same relationship-scoped
/// (Own/Managed/All) model as the rest of the staff profile.
/// </summary>
public class StaffAttachmentsService(
    IAuthorizationService authorizationService,
    ILogger<StaffAttachmentsService> logger,
    IDirectoryService directoryService,
    IDocumentService documentService,
    IValidationService validationService,
    IStaffMemberAccessService accessService,
    IStaffMemberRepository staffMemberRepository,
    IPersonRepository personRepository)
    : DirectoryEntityService<Person>(authorizationService, logger, directoryService, documentService,
        validationService), IStaffAttachmentsService
{
    private const StaffAccess ViewAccess =
        StaffAccess.ViewOwn | StaffAccess.ViewManaged | StaffAccess.ViewAll;

    private const StaffAccess EditAccess =
        StaffAccess.EditOwn | StaffAccess.EditManaged | StaffAccess.EditAll;

    // entityId is the StaffMember id; resolve to the owning Person (root-directory holder).
    public override async Task<Person> GetByIdAsync(Guid entityId, CancellationToken cancellationToken)
    {
        var staffMember = await staffMemberRepository.GetByIdAsync(entityId, cancellationToken)
            ?? throw new NotFoundException("Staff member not found.");

        return await personRepository.GetByIdAsync(staffMember.PersonId, cancellationToken)
            ?? throw new NotFoundException("Person not found.");
    }

    public override async Task<bool> CanViewDirectoryAsync(Guid entityId, Guid directoryId, CancellationToken ct)
        => await accessService.CanAsync(entityId, StaffArea.Documents, ViewAccess, ct)
           && await CanStructurallyViewDirectoryAsync(entityId, directoryId, ct);

    public override async Task<bool> CanEditDirectoryAsync(Guid entityId, Guid directoryId, CancellationToken ct)
        => await accessService.CanAsync(entityId, StaffArea.Documents, EditAccess, ct)
           && await CanStructurallyEditDirectoryAsync(entityId, directoryId, ct);

    public override async Task<bool> CanUploadToDirectoryAsync(Guid entityId, Guid directoryId, CancellationToken ct)
        => await accessService.CanAsync(entityId, StaffArea.Documents, EditAccess, ct)
           && await CanStructurallyUploadToDirectoryAsync(entityId, directoryId, ct);

    public override async Task<bool> CanEditDocumentAsync(Guid entityId, DocumentDetailsResponse document,
        CancellationToken ct)
        => await accessService.CanAsync(entityId, StaffArea.Documents, EditAccess, ct)
           && await CanStructurallyViewDirectoryAsync(entityId, document.DirectoryId, ct);
}
