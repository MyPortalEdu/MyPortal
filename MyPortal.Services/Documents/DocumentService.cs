using MyPortal.Auth.Interfaces;
using MyPortal.Common.Enums;
using MyPortal.Common.Exceptions;
using MyPortal.Contracts.Models;
using MyPortal.Contracts.Models.Documents;
using MyPortal.Core.Entities;
using MyPortal.FileStorage.Helpers;
using MyPortal.FileStorage.Interfaces;
using MyPortal.Services.Filters;
using MyPortal.Services.Interfaces;
using MyPortal.Services.Interfaces.Repositories;
using MyPortal.Services.Interfaces.Services;
using QueryKit.Sql;
using Task = System.Threading.Tasks.Task;

namespace MyPortal.Services.Documents
{
    /// <inheritdoc cref="IDocumentService"/>
    public class DocumentService : BaseService, IDocumentService
    {
        private readonly IDocumentRepository _documentRepository;
        private readonly IDocumentTypeRepository _documentTypeRepository;
        private readonly IStorageKeyGenerator _storageKeyGenerator;
        private readonly IFileStorageProvider _storageProvider;
        private readonly IValidationService _validationService;

        
        public DocumentService(IAuthorizationService authorizationService, IDocumentRepository documentRepository,
            IDocumentTypeRepository documentTypeRepository,
            IStorageKeyGenerator storageKeyGenerator, IFileStorageProvider storageProvider,
            IValidationService validationService) : base(authorizationService)
        {
            _documentRepository = documentRepository;
            _documentTypeRepository = documentTypeRepository;
            _storageKeyGenerator = storageKeyGenerator;
            _storageProvider = storageProvider;
            _validationService = validationService;
        }
        
        public async Task<DocumentDetailsResponse> CreateDocumentAsync(DocumentUpsertRequest model,
            CancellationToken cancellationToken)
        {
            if (model.Content == null || model.SizeBytes <= 0 || !model.Content.CanRead)
            {
                throw new ArgumentException("Document has no content.", nameof(model.Content));
            }

            await _validationService.ValidateAsync(model);

            if (model.IsPrivate && _authorizationService.GetCurrentUserType() != UserType.Staff)
            {
                throw new ForbiddenException("You do not have permission to create private documents.");
            }

            var storageKey = _storageKeyGenerator.Generate(model.FileName!);

            await using var hashedStream =
                await FileStorageHasher.HashAndPrepareStreamAsync(model.Content, cancellationToken);

            await _storageProvider.SaveFileAsync(storageKey, hashedStream.UsableStream, model.ContentType!,
                cancellationToken);

            var id = SqlConvention.SequentialGuid();

            var document = new Document
            {
                Id = id,
                StorageKey = storageKey,
                ContentType = model.ContentType!,
                FileName = model.FileName!,
                DirectoryId = model.DirectoryId,
                SizeBytes = model.SizeBytes,
                TypeId = model.TypeId,
                Title = model.Title,
                Description = model.Description,
                Hash = hashedStream.Hash,
                IsPrivate = model.IsPrivate
            };

            await _documentRepository.InsertAsync(document, cancellationToken);

            var response = await _documentRepository.GetDetailsByIdAsync(id, cancellationToken)
                           ?? throw new InvalidOperationException("Created document, but could not load details.");

            return response;
        }

        public async Task<DocumentDetailsResponse> UpdateDocumentAsync(Guid documentId, DocumentUpsertRequest model,
            CancellationToken cancellationToken)
        {
            await _validationService.ValidateAsync(model);
            
            var documentInDb = await _documentRepository.GetByIdAsync(documentId, cancellationToken);

            if (documentInDb == null)
            {
                throw new NotFoundException("Document not found.");
            }

            if (model.Content != null)
            {
                await using var hashedStream =
                    await FileStorageHasher.HashAndPrepareStreamAsync(model.Content, cancellationToken);
                await _storageProvider.SaveFileAsync(documentInDb.StorageKey, hashedStream.UsableStream,
                    model.ContentType!, cancellationToken);
                documentInDb.FileName = model.FileName!;
                documentInDb.ContentType = model.ContentType!;
                documentInDb.SizeBytes = model.SizeBytes;
                documentInDb.Hash = hashedStream.Hash;
            }

            documentInDb.TypeId = model.TypeId;
            documentInDb.DirectoryId = model.DirectoryId;
            documentInDb.Title = model.Title;
            documentInDb.Description = model.Description;
            documentInDb.IsPrivate = model.IsPrivate;

            if (documentInDb.IsPrivate && _authorizationService.GetCurrentUserType() != UserType.Staff)
            {
                throw new ForbiddenException("You do not have permission to make documents private.");
            }

            await _documentRepository.UpdateAsync(documentInDb, cancellationToken);

            var response = await _documentRepository.GetDetailsByIdAsync(documentId, cancellationToken)
                           ?? throw new InvalidOperationException("Updated document, but could not load details.");

            return response;
        }
        
        public async Task DeleteDocumentAsync(Guid documentId, CancellationToken cancellationToken, bool softDelete = true)
        {
            var document = await _documentRepository.GetByIdAsync(documentId, cancellationToken);

            if (document == null)
            {
                throw new NotFoundException("Document not found.");
            }

            if (document.IsPrivate && _authorizationService.GetCurrentUserType() != UserType.Staff)
            {
                throw new ForbiddenException("You do not have permission to delete this document.");
            }

            if (!softDelete)
            {
                await _storageProvider.DeleteFileAsync(document.StorageKey, cancellationToken);
            }
            
            await _documentRepository.DeleteAsync(documentId, cancellationToken, softDelete);
        }

        public async Task<DocumentDetailsResponse?> GetDocumentByIdAsync(Guid documentId,
            CancellationToken cancellationToken)
        {
            return await _documentRepository.GetDetailsByIdAsync(documentId, cancellationToken);
        }

        public async Task<DocumentContentResponse> GetDocumentWithContentByIdAsync(Guid documentId,
            CancellationToken cancellationToken)
        {
            var documentDetails = await _documentRepository.GetDetailsByIdAsync(documentId, cancellationToken);

            if (documentDetails == null)
            {
                throw new NotFoundException("Document not found.");
            }

            if (documentDetails.IsPrivate && _authorizationService.GetCurrentUserType() != UserType.Staff)
            {
                throw new ForbiddenException("You do not have permission to view this document.");
            }

            // The caller is responsible for disposing the returned stream
            // We use try-catch to ensure proper disposal if an exception occurs after opening the stream
            Stream? content = null;
            try
            {
                content = await _storageProvider.OpenReadFileAsync(documentDetails.StorageKey, cancellationToken);
                
                var response = new DocumentContentResponse
                {
                    Details = documentDetails,
                    Content = content
                };

                return response;
            }
            catch (Exception)
            {
                if (content != null)
                {
                    await content.DisposeAsync();
                }
                
                throw;
            }
        }

        public async Task<IList<LookupResponse>> GetDocumentTypesAsync(DocumentTypeFilter filter,
            CancellationToken cancellationToken)
        {
            return await _documentTypeRepository.GetDocumentTypes(filter, cancellationToken);
        }
    }
}