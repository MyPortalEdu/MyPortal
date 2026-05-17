using Microsoft.Extensions.Logging;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Enums;
using MyPortal.Common.Exceptions;
using MyPortal.Common.Interfaces;
using MyPortal.Contracts.Models.Documents;
using MyPortal.Data.Interfaces;
using MyPortal.Services.Extensions;
using MyPortal.Services.Interfaces.Documents;
using QueryKit.Sql;
using Directory = MyPortal.Core.Entities.Directory;

namespace MyPortal.Services.Documents;

public class DirectoryService : BaseService, IDirectoryService
{
    private readonly IDirectoryRepository _directoryRepository;
    private readonly IDocumentService _documentService;
    private readonly IUnitOfWorkFactory _unitOfWorkFactory;

    public DirectoryService(IAuthorizationService authorizationService, ILogger<DirectoryService> logger,
        IDirectoryRepository directoryRepository, IDocumentService documentService,
        IUnitOfWorkFactory unitOfWorkFactory) : base(authorizationService, logger)
    {
        _directoryRepository = directoryRepository;
        _documentService = documentService;
        _unitOfWorkFactory = unitOfWorkFactory;
    }

    public async Task<DirectoryDetailsResponse> CreateAsync(DirectoryUpsertRequest model,
        CancellationToken cancellationToken, IUnitOfWork? uow = null)
    {
        RequireStaff("create");

        var id = SqlConvention.SequentialGuid();

        var directory = new Directory
        {
            Id = id,
            Name = model.Name,
            ParentId = model.ParentId,
            IsPrivate = model.IsPrivate,
            UploadPolicy = model.UploadPolicy
        };

        await _directoryRepository.InsertAsync(directory, cancellationToken, uow?.Transaction);

        return await GetDirectoryByIdAsync(id, cancellationToken, uow);
    }

    public async Task<DirectoryDetailsResponse> UpdateAsync(Guid directoryId, DirectoryUpsertRequest model,
        CancellationToken cancellationToken)
    {
        RequireStaff("update");

        var directory = await _directoryRepository.GetByIdAsync(directoryId, cancellationToken);

        if (directory == null)
        {
            throw new NotFoundException("Directory not found.");
        }

        directory.IsPrivate = model.IsPrivate;
        directory.Name = model.Name;
        directory.ParentId = model.ParentId;
        directory.UploadPolicy = model.UploadPolicy;

        await _directoryRepository.UpdateAsync(directory, cancellationToken);

        return await GetDirectoryByIdAsync(directoryId, cancellationToken);
    }

    public async Task<DirectoryDetailsResponse> GetDirectoryByIdAsync(Guid directoryId,
        CancellationToken cancellationToken, IUnitOfWork? uow = null)
    {
        var result = await _directoryRepository.GetDetailsByIdAsync(directoryId,
            cancellationToken, uow?.Transaction);

        if (result == null)
        {
            throw new NotFoundException("Directory not found.");
        }

        return result;
    }

    public async Task<DirectoryDetailsResponse?> TryGetDirectoryByIdAsync(Guid directoryId,
        CancellationToken cancellationToken)
    {
        var result = await _directoryRepository.GetDetailsByIdAsync(directoryId, cancellationToken);

        return result;
    }

    public async Task<DirectoryContentsResponse> GetDirectoryContentsAsync(Guid directoryId,
        CancellationToken cancellationToken)
    {
        var directory = await GetDirectoryByIdAsync(directoryId, cancellationToken);

        var directories = await _directoryRepository.GetDirectoriesByParentIdAsync(directoryId, cancellationToken);
        var documents = await _documentService.GetDocumentsByDirectoryId(directoryId, cancellationToken);

        var response = new DirectoryContentsResponse(directory, directories, documents);

        return response;
    }

    public async Task<DirectoryTreeResponse> GetDirectoryTreeAsync(
        Guid directoryId,
        CancellationToken cancellationToken,
        bool includeDeletedDocs = false)
    {
        var flatTree = await GetFlatDirectoryTreeAsync(directoryId, cancellationToken, includeDeletedDocs);
        return BuildTree(directoryId, flatTree);
    }

    public async Task<DirectoryContentsResponse> GetFlatDirectoryTreeAsync(Guid directoryId,
        CancellationToken cancellationToken, bool includeDeletedDocs = false)
    {
        var directory = await GetDirectoryByIdAsync(directoryId, cancellationToken);

        var directories = await _directoryRepository.GetDirectoryTreeAsync(directoryId, cancellationToken);

        var documents = await _documentService.GetDocumentsInSubtreeAsync(directoryId, cancellationToken,
            includeDeletedDocs);

        return new DirectoryContentsResponse(directory, directories, documents);
    }

    public async Task<bool> IsDirectoryInSubtreeAsync(Guid rootDirectoryId, Guid candidateDirectoryId,
        CancellationToken cancellationToken)
    {
        return await _directoryRepository.IsInSubtreeAsync(rootDirectoryId, candidateDirectoryId, cancellationToken);
    }
    
    public async Task DeleteAsync(Guid directoryId, CancellationToken cancellationToken,
        IUnitOfWork? uow = null, bool softDelete = true)
    {
        RequireStaff("delete");

        var directory = await _directoryRepository.GetDetailsByIdAsync(directoryId, cancellationToken,
            uow?.Transaction);

        if (directory == null)
        {
            throw new NotFoundException("Directory not found.");
        }

        // Reject the delete when this directory is the root of an IDirectoryEntity
        // (Bulletin, Agency, Class, Document, HomeworkItem, LessonPlan, Person).
        // The FK constraint on each owner table is the hard backstop for a literal
        // DELETE row, but soft-delete leaves the row in place and wouldn't trip the
        // FK — this check covers that case and produces a friendlier message.
        // Reads the in-flight transaction so owner-service cleanups (which delete
        // the owner row first, then ask us to wipe the directory) see the cleared
        // reference and proceed.
        var owner = await _directoryRepository.GetReferencingOwnerAsync(directoryId, cancellationToken,
            uow?.Transaction);

        if (owner != null)
        {
            throw new EntityInUseException(
                $"This directory belongs to a {owner.OwnerType} and cannot be deleted directly. " +
                $"Delete the {owner.OwnerType} instead.");
        }

        // Wrap the recursive walk + the root delete in one transaction so a failure halfway
        // through (cancellation, repo error) rolls back instead of leaving a half-deleted tree.
        await _unitOfWorkFactory.RunInTransactionAsync(uow, async activeUow =>
        {
            await DeleteDirectoryContentsAsync(directoryId, cancellationToken, activeUow, softDelete);

            await _directoryRepository.DeleteAsync(directoryId, cancellationToken,
                softDelete, activeUow.Transaction);
        }, cancellationToken);
    }

    private async Task DeleteDirectoryContentsAsync(Guid directoryId, CancellationToken cancellationToken,
        IUnitOfWork uow, bool softDelete = true)
    {
        var contents = await GetDirectoryContentsAsync(directoryId, cancellationToken);

        foreach (var subdirectory in contents.Directories)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await DeleteDirectoryContentsAsync(subdirectory.Id, cancellationToken, uow, softDelete);

            await _directoryRepository.DeleteAsync(subdirectory.Id, cancellationToken, softDelete, uow.Transaction);
        }

        foreach (var document in contents.Documents)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await _documentService.DeleteAsync(document.Id, cancellationToken,
                softDelete, uow);
        }
    }


    private DirectoryTreeResponse BuildTree(Guid rootDirectoryId, DirectoryContentsResponse flatTree)
    {
        var directoriesById = flatTree.Directories.ToDictionary(d => d.Id);

        // Key by non-null ParentId (Guid), ignore roots (ParentId == null)
        var directoriesByParent = flatTree.Directories
            .Where(d => d.ParentId.HasValue)
            .GroupBy(d => d.ParentId!.Value)
            .ToDictionary(g => g.Key, g => g.ToList());

        var documentsByDirectory = flatTree.Documents
            .GroupBy(d => d.DirectoryId)
            .ToDictionary(g => g.Key, g => g.ToList());

        DirectoryTreeResponse Build(Guid dirId, HashSet<Guid> visited)
        {
            if (!visited.Add(dirId))
                throw new InvalidOperationException($"Cycle detected at directory {dirId}");

            if (!directoriesById.TryGetValue(dirId, out var dir))
                throw new InvalidOperationException($"Directory {dirId} not found.");

            directoriesByParent.TryGetValue(dirId, out var childDirs);
            documentsByDirectory.TryGetValue(dirId, out var childDocs);

            childDirs ??= new List<DirectoryDetailsResponse>();
            childDocs ??= new List<DocumentDetailsResponse>();

            // IMPORTANT: don't reuse the same visited set across siblings
            // or we'll get false "cycle detected" when different branches share an ancestor
            return new DirectoryTreeResponse(
                dir,
                childDirs.Select(d => Build(d.Id, new HashSet<Guid>(visited))).ToList(),
                childDocs
            );
        }

        return Build(rootDirectoryId, new HashSet<Guid>());
    }

    private void RequireStaff(string action)
    {
        if (AuthorizationService.GetCurrentUserType() != UserType.Staff)
        {
            throw new ForbiddenException($"You do not have permission to {action} this directory.");
        }
    }
}
