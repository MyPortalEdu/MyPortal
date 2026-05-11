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
    private readonly IDocumentRepository _documentRepository;
    private readonly IUnitOfWorkFactory _unitOfWorkFactory;

    public DirectoryService(IAuthorizationService authorizationService, ILogger<DirectoryService> logger,
        IDirectoryRepository directoryRepository, IDocumentRepository documentRepository,
        IUnitOfWorkFactory unitOfWorkFactory) : base(authorizationService, logger)
    {
        _directoryRepository = directoryRepository;
        _documentRepository = documentRepository;
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

    public async Task DeleteAsync(Guid directoryId, CancellationToken cancellationToken,
        IUnitOfWork? uow = null)
    {
        RequireStaff("delete");

        var directory = await _directoryRepository.GetDetailsByIdAsync(directoryId, cancellationToken);

        if (directory == null)
        {
            throw new NotFoundException("Directory not found.");
        }

        // Wrap the recursive walk + the root delete in one transaction so a failure halfway
        // through (cancellation, repo error) rolls back instead of leaving a half-deleted tree.
        // Note: this path always soft-deletes documents (via the repo default). If hard-delete
        // with blob cleanup is ever needed here, route through IDocumentService.DeleteAsync
        // and trigger storage cleanup after this transaction commits.
        await _unitOfWorkFactory.RunInTransactionAsync(uow, async activeUow =>
        {
            await DeleteDirectoryContentsAsync(directoryId, cancellationToken, activeUow);

            await _directoryRepository.DeleteAsync(directoryId, cancellationToken,
                transaction: activeUow.Transaction);
        }, cancellationToken);
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
        var documents = await _documentRepository.GetDocumentsByDirectoryId(directoryId, cancellationToken);

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

        var documents = await _documentRepository.GetDocumentsInSubtreeAsync(directoryId, cancellationToken,
            includeDeletedDocs);

        return new DirectoryContentsResponse(directory, directories, documents);
    }

    public async Task<bool> IsDirectoryInSubtreeAsync(Guid rootDirectoryId, Guid candidateDirectoryId,
        CancellationToken cancellationToken)
    {
        return await _directoryRepository.IsInSubtreeAsync(rootDirectoryId, candidateDirectoryId, cancellationToken);
    }

    private async Task DeleteDirectoryContentsAsync(Guid directoryId, CancellationToken cancellationToken,
        IUnitOfWork uow)
    {
        var contents = await GetDirectoryContentsAsync(directoryId, cancellationToken);

        foreach (var subdirectory in contents.Directories)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await DeleteDirectoryContentsAsync(subdirectory.Id, cancellationToken, uow);

            await _directoryRepository.DeleteAsync(subdirectory.Id, cancellationToken,
                transaction: uow.Transaction);
        }

        foreach (var document in contents.Documents)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await _documentRepository.DeleteAsync(document.Id, cancellationToken,
                transaction: uow.Transaction);
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
