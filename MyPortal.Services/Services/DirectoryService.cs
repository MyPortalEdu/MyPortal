using MyPortal.Auth.Interfaces;
using MyPortal.Common.Enums;
using MyPortal.Common.Exceptions;
using MyPortal.Contracts.Models.Documents;
using MyPortal.Services.Extensions;
using MyPortal.Services.Interfaces.Repositories;
using MyPortal.Services.Interfaces.Services;
using QueryKit.Sql;
using Directory = MyPortal.Core.Entities.Directory;

namespace MyPortal.Services.Services;

public class DirectoryService : BaseService, IDirectoryService
{
    private readonly IDirectoryRepository _directoryRepository;
    private readonly IDocumentRepository _documentRepository;

    public DirectoryService(IAuthorizationService authorizationService, IDirectoryRepository directoryRepository,
        IDocumentRepository documentRepository) : base(authorizationService)
    {
        _directoryRepository = directoryRepository;
        _documentRepository = documentRepository;
    }

    public async Task<DirectoryDetailsResponse> CreateDirectoryAsync(DirectoryUpsertRequest model,
        CancellationToken cancellationToken)
    {
        if (_authorizationService.GetCurrentUserType() != UserType.Staff && model.IsPrivate)
        {
            throw new ForbiddenException("You do not have permission to create private directories.");
        }

        var id = SqlConvention.SequentialGuid();

        var directory = new Directory
        {
            Id = id,
            Name = model.Name,
            ParentId = model.ParentId,
            IsPrivate = model.IsPrivate
        };

        await _directoryRepository.InsertAsync(directory, cancellationToken);

        return await GetDirectoryByIdAsync(id, cancellationToken) ??
               throw new InvalidOperationException("Created directory, but could not load details.");
    }

    public async Task<DirectoryDetailsResponse> UpdateDirectoryAsync(Guid directoryId, DirectoryUpsertRequest model,
        CancellationToken cancellationToken)
    {
        var directory = await _directoryRepository.GetByIdAsync(directoryId, cancellationToken);

        if (directory == null)
        {
            throw new NotFoundException("Directory not found.");
        }

        if (_authorizationService.GetCurrentUserType() != UserType.Staff && model.IsPrivate)
        {
            throw new ForbiddenException("You do not have permission to make private directories.");
        }

        directory.IsPrivate = model.IsPrivate;
        directory.Name = model.Name;
        directory.ParentId = model.ParentId;

        await _directoryRepository.UpdateAsync(directory, cancellationToken);

        return await GetDirectoryByIdAsync(directoryId, cancellationToken) ??
               throw new InvalidOperationException("Updated directory, but could not load details.");
    }

    public async Task DeleteDirectoryAsync(Guid directoryId, CancellationToken cancellationToken)
    {
        if (_authorizationService.GetCurrentUserType() != UserType.Staff)
        {
            var tree = await GetDirectoryTreeAsync(directoryId, cancellationToken);

            if (tree.ContainsPrivateEntities())
            {
                throw new ForbiddenException("You do not have permission to delete this directory.");
            }
        }
        
        var directory = await _directoryRepository.GetDetailsByIdAsync(directoryId, cancellationToken);

        if (directory == null)
        {
            throw new NotFoundException("Directory not found.");
        }

        await _directoryRepository.DeleteAsync(directoryId, cancellationToken);
    }

    public async Task<DirectoryDetailsResponse?> GetDirectoryByIdAsync(Guid directoryId,
        CancellationToken cancellationToken)
    {
        var result = await _directoryRepository.GetDetailsByIdAsync(directoryId, cancellationToken);

        if (result is { IsPrivate: true } && _authorizationService.GetCurrentUserType() != UserType.Staff)
        {
            throw new ForbiddenException("You do not have permission to view this directory.");
        }

        return result;
    }

    public async Task<DirectoryContentsResponse> GetDirectoryContentsAsync(Guid directoryId,
        CancellationToken cancellationToken)
    {
        var directory = await GetDirectoryByIdAsync(directoryId, cancellationToken);

        if (directory == null)
        {
            throw new NotFoundException("Directory not found.");
        }

        if (directory.IsPrivate && _authorizationService.GetCurrentUserType() != UserType.Staff)
        {
            throw new ForbiddenException("You do not have permission to view this directory.");
        }

        var directories = await _directoryRepository.GetDirectoriesByParentIdAsync(directoryId, cancellationToken);
        var documents = await _documentRepository.GetDocumentsByDirectoryId(directoryId, cancellationToken);

        var response = new DirectoryContentsResponse(directory,
            directories.Where(d => _authorizationService.GetCurrentUserType() == UserType.Staff || !d.IsPrivate)
                .ToList(),
            documents.Where(e => _authorizationService.GetCurrentUserType() == UserType.Staff || !e.IsPrivate)
                .ToList());

        return response;
    }
    
    public async Task<DirectoryTreeResponse> GetDirectoryTreeAsync(
        Guid directoryId,
        CancellationToken cancellationToken,
        bool includeDeletedDocs = true)
    {
        var directories = await _directoryRepository.GetChildDirectoriesAsync(directoryId, cancellationToken);

        IReadOnlyList<DocumentDetailsResponse> documents = Array.Empty<DocumentDetailsResponse>();
        if (includeDeletedDocs)
        {
            documents = await _documentRepository.GetChildDocumentsByDirectoryId(directoryId, cancellationToken,
                includeDeletedDocs);
        }

        var directoriesByParent = directories
            .GroupBy(d => d.ParentId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var documentsByDirectory = documents
            .GroupBy(d => d.DirectoryId)
            .ToDictionary(g => g.Key, g => g.ToList());

        DirectoryTreeResponse BuildTree(Guid rootDirectoryId)
        {
            var dir = directories.FirstOrDefault(d => d.Id == rootDirectoryId)
                      ?? throw new InvalidOperationException($"Directory {rootDirectoryId} not found.");

            directoriesByParent.TryGetValue(rootDirectoryId, out var childDirs);
            documentsByDirectory.TryGetValue(rootDirectoryId, out var childDocs);

            childDirs ??= new List<DirectoryDetailsResponse>();
            childDocs ??= new List<DocumentDetailsResponse>();

            return new DirectoryTreeResponse(
                dir,
                childDirs.Select(d => BuildTree(d.Id)).ToList(),
                childDocs
            );
        }

        return BuildTree(directoryId);
    }

}