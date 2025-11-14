using MyPortal.Auth.Interfaces;
using MyPortal.Common.Exceptions;
using MyPortal.Contracts.Models.Documents;
using MyPortal.Core.Entities;
using MyPortal.FileStorage.Helpers;
using MyPortal.FileStorage.Interfaces;
using MyPortal.Services.Interfaces;
using MyPortal.Services.Interfaces.Repositories;
using MyPortal.Services.Interfaces.Services;
using QueryKit.Sql;
using Task = System.Threading.Tasks.Task;

namespace MyPortal.Services.Services
{
    /// <inheritdoc cref="IDocumentService"/>
    public class DocumentService : BaseService, IDocumentService
    {
        private readonly IDocumentRepository _documentRepository;
        private readonly IStorageKeyGenerator _storageKeyGenerator;
        private readonly IFileStorageProvider _storageProvider;
        private readonly IValidationService _validationService;

        /// <summary>
        /// Instantiates a new instance of <see cref="DocumentService"/>.
        /// </summary>
        /// <param name="authorizationService">Service used to validate auth requirements.</param>
        /// <param name="documentRepository">Repository used to retrieve documents from storage.</param>
        /// <param name="storageKeyGenerator">Service used to generate storage keys.</param>
        /// <param name="storageProvider">Service used to retrieve document contents from file storage.</param>
        /// <param name="validationService">Service used to validate reqeusts.</param>
        public DocumentService(IAuthorizationService authorizationService, IDocumentRepository documentRepository,
            IStorageKeyGenerator storageKeyGenerator, IFileStorageProvider storageProvider,
            IValidationService validationService) : base(authorizationService)
        {
            _documentRepository = documentRepository;
            _storageKeyGenerator = storageKeyGenerator;
            _storageProvider = storageProvider;
            _validationService = validationService;
        }
        
        /// <inheritdoc/>
        /// <exception cref="ArgumentException">Thrown if there is no content to upload.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the document details cannot be loaded after creation.</exception>
        public async Task<DocumentDetailsResponse> CreateDocumentAsync(DocumentUpsertRequest model,
            CancellationToken cancellationToken)
        {
            if (model.Content == null || model.Content.Length == 0)
            {
                throw new ArgumentException("Document has no content.", nameof(model.Content));
            }

            await _validationService.ValidateAsync(model);

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

        /// <inheritdoc/>
        /// <exception cref="NotFoundException">Thrown if a document with the specified identifier is not found.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the document details cannot be loaded after update.</exception>
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

            await _documentRepository.UpdateAsync(documentInDb, cancellationToken);

            var response = await _documentRepository.GetDetailsByIdAsync(documentId, cancellationToken)
                           ?? throw new InvalidOperationException("Updated document, but could not load details.");

            return response;
        }

        // TODO: Add a non-soft delete overload for maintenance routines
        /// <inheritdoc/>
        /// <exception cref="NotFoundException">Thrown if a document with the specified identifier is not found.</exception>
        public async Task DeleteDocumentAsync(Guid documentId, CancellationToken cancellationToken)
        {
            var document = await _documentRepository.GetByIdAsync(documentId, cancellationToken);

            if (document == null)
            {
                throw new NotFoundException("Document not found.");
            }

            await _documentRepository.DeleteAsync(documentId, cancellationToken);
        }

        public async Task<DocumentDetailsResponse?> GetDocumentByIdAsync(Guid documentId,
            CancellationToken cancellationToken)
        {
            return await _documentRepository.GetDetailsByIdAsync(documentId, cancellationToken);
        }

        /// <inheritdoc/>
        /// <exception cref="NotFoundException">Thrown if a document with the specified identifier is not found.</exception>
        public async Task<DocumentContentResponse> GetDocumentWithContentByIdAsync(Guid documentId,
            CancellationToken cancellationToken)
        {
            var documentDetails = await _documentRepository.GetDetailsByIdAsync(documentId, cancellationToken);

            if (documentDetails == null)
            {
                throw new NotFoundException("Document not found.");
            }

            // Do NOT dispose this stream here - it's returned to the caller who must dispose it
            var content = await _storageProvider.OpenReadFileAsync(documentDetails.StorageKey, cancellationToken);

            var response = new DocumentContentResponse
            {
                Details = documentDetails,
                Content = content
            };

            return response;
        }
    }
}