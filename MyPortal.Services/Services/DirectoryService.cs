using MyPortal.Auth.Interfaces;
using MyPortal.Common.Enums;
using MyPortal.Common.Exceptions;
using MyPortal.Contracts.Models.Documents;
using MyPortal.Services.Interfaces.Repositories;
using MyPortal.Services.Interfaces.Services;
using QueryKit.Sql;
using Directory = MyPortal.Core.Entities.Directory;

namespace MyPortal.Services.Services
{
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

        public async Task<DirectoryDetailsResponse> CreateDirectoryAsync(DirectoryUpsertRequest model, CancellationToken cancellationToken)
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

            return await GetDirectoryByIdAsync(id, cancellationToken) ?? throw new InvalidOperationException("Created directory, but could not load details.");
        }

        public async Task<DirectoryDetailsResponse> UpdateDirectoryAsync(Guid directoryId, DirectoryUpsertRequest model, CancellationToken cancellationToken)
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

            return await GetDirectoryByIdAsync(directoryId, cancellationToken) ?? throw new InvalidOperationException("Updated directory, but could not load details.");
        }

        // TODO: Add a non-soft delete overload for maintenance routines
        public async Task DeleteDirectoryAsync(Guid directoryId, CancellationToken cancellationToken)
        {
            var directory = await _directoryRepository.GetDetailsByIdAsync(directoryId, cancellationToken);

            if (directory == null)
            {
                throw new NotFoundException("Directory not found.");
            }

            await _directoryRepository.DeleteAsync(directoryId, cancellationToken);
        }

        public async Task<DirectoryDetailsResponse?> GetDirectoryByIdAsync(Guid directoryId, CancellationToken cancellationToken)
        {
            return await _directoryRepository.GetDetailsByIdAsync(directoryId, cancellationToken);
        }

        public async Task<DirectoryContentsResponse> GetDirectoryContentsAsync(Guid directoryId, CancellationToken cancellationToken)
        {
            var directory = await GetDirectoryByIdAsync(directoryId, cancellationToken);

            if (directory == null)
            {
                throw new NotFoundException("Directory not found.");
            }

            var directories = await _directoryRepository.GetDirectoriesByParentIdAsync(directoryId, cancellationToken);
            var documents = await _documentRepository.GetDocumentsByDirectoryId(directoryId, cancellationToken);

            var response = new DirectoryContentsResponse
            {
                Directory = directory,
                Directories = directories,
                Documents = documents
            };

            return response;
        }
    }
}
