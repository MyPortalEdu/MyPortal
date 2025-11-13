using MyPortal.Auth.Interfaces;
using MyPortal.Common.Exceptions;
using MyPortal.Contracts.Models.Documents;
using MyPortal.Core.Entities;
using MyPortal.FileStorage.Helpers;
using MyPortal.FileStorage.Interfaces;
using MyPortal.Services.Interfaces.Repositories;
using MyPortal.Services.Interfaces.Services;
using QueryKit.Sql;
using Task = System.Threading.Tasks.Task;

namespace MyPortal.Services.Services
{
    public class DocumentService : BaseService, IDocumentService
    {
        private readonly IDocumentRepository _documentRepository;
        private readonly IStorageKeyGenerator _storageKeyGenerator;
        private readonly IFileStorageProvider _storageProvider;

        public DocumentService(IAuthorizationService authorizationService, IDocumentRepository documentRepository,
            IStorageKeyGenerator storageKeyGenerator, IFileStorageProvider storageProvider) : base(authorizationService)
        {
            _documentRepository = documentRepository;
            _storageKeyGenerator = storageKeyGenerator;
            _storageProvider = storageProvider;
        }


        public async Task<DocumentDetailsResponse> CreateDocumentAsync(DocumentUpsertRequest model, CancellationToken cancellationToken)
        {
            if (model.Content == null)
            {
                throw new ArgumentException("Document has no content.", nameof(model.Content));
            }

            var storageKey = _storageKeyGenerator.Generate(model.FileName);

            var hashedStream = await FileStorageHasher.HashAndPrepareStreamAsync(model.Content, cancellationToken);

            await _storageProvider.SaveFileAsync(storageKey, hashedStream.UsableStream, model.ContentType, cancellationToken);

            var id = SqlConvention.SequentialGuid();

            var document = new Document
            {
                Id = id,
                StorageKey = storageKey,
                ContentType = model.ContentType,
                FileName = model.FileName,
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

            return response!;
        }

        public async Task<DocumentDetailsResponse> UpdateDocumentAsync(Guid documentId, DocumentUpsertRequest model, CancellationToken cancellationToken)
        {
            var documentInDb = await _documentRepository.GetByIdAsync(documentId, cancellationToken);

            if (documentInDb == null)
            {
                throw new NotFoundException("Document not found.");
            }

            if (model.Content != null)
            {
                var hashedStream = await FileStorageHasher.HashAndPrepareStreamAsync(model.Content, cancellationToken);
                await _storageProvider.SaveFileAsync(documentInDb.StorageKey, hashedStream.UsableStream, model.ContentType, cancellationToken);
                documentInDb.FileName = model.FileName;
                documentInDb.ContentType = model.ContentType;
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

            return response!;
        }

        public async Task DeleteDocumentAsync(Guid documentId, CancellationToken cancellationToken)
        {
            var document = await _documentRepository.GetByIdAsync(documentId, cancellationToken);

            if (document == null)
            {
                throw new NotFoundException("Document not found.");
            }

            await _documentRepository.DeleteAsync(documentId, cancellationToken);
        }

        public async Task<DocumentDetailsResponse?> GetDocumentByIdAsync(Guid documentId, CancellationToken cancellationToken)
        {
            return await _documentRepository.GetDetailsByIdAsync(documentId, cancellationToken);
        }

        public async Task<DocumentContentResponse> GetDocumentWithContentByIdAsync(Guid documentId, CancellationToken cancellationToken)
        {
            var documentDetails = await _documentRepository.GetDetailsByIdAsync(documentId, cancellationToken);
            
            if (documentDetails == null)
            {
                throw new NotFoundException("Document not found.");
            }

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
