﻿using System;
using System.Threading.Tasks;
using MyPortal.Logic.Exceptions;
using MyPortal.Logic.Interfaces;
using MyPortal.Logic.Interfaces.Services;
using MyPortal.Logic.Models.DocumentProvision;
using MyPortal.Logic.Models.Requests.Documents;

namespace MyPortal.Logic.Services
{
    public sealed class LocalFileService : BaseService, IFileService
    {
        private readonly ILocalFileProvider _fileProvider;
        private readonly IDocumentAccessController _documentAccessController;

        public LocalFileService(ISessionUser sessionUser, ILocalFileProvider fileProvider,
            IDocumentAccessController documentAccessController) : base(sessionUser)
        {
            _fileProvider = fileProvider;
            _documentAccessController = documentAccessController;
        }

        public async Task UploadFileToDocument(FileUploadRequestModel upload)
        {
            await _documentAccessController.VerifyDocumentAccess(upload.DocumentId, true);
            
            await using var unitOfWork = await User.GetConnection();

            var document = await unitOfWork.Documents.GetById(upload.DocumentId);

            if (document == null)
            {
                throw new NotFoundException("Document not found.");
            }

            if (document.FileId.HasValue)
            {
                throw new LogicException("A file is already attached to this document.");
            }

            var file = await _fileProvider.SaveFile(upload);

            document.Attachment = file;

            await unitOfWork.Documents.Update(document);

            await unitOfWork.SaveChangesAsync();
        }

        public async Task<FileDownload> GetDownloadByDocument(Guid documentId)
        {
            await _documentAccessController.VerifyDocumentAccess(documentId, false);
            
            await using var unitOfWork = await User.GetConnection();

            var file = await unitOfWork.Files.GetByDocumentId(documentId);

            var stream = await _fileProvider.LoadFileAsStream(file.FileId);

            return new FileDownload(stream, file.ContentType, file.FileName);
        }

        public async Task RemoveFileFromDocument(Guid documentId)
        {
            await _documentAccessController.VerifyDocumentAccess(documentId, true);
            
            await using var unitOfWork = await User.GetConnection();

            var file = await unitOfWork.Files.GetByDocumentId(documentId);

            if (file == null)
            {
                throw new LogicException("No file is attached to this document.");
            }

            _fileProvider.DeleteFile(file.FileId);

            await unitOfWork.Files.Delete(file.Id);

            await unitOfWork.SaveChangesAsync();
        }
    }
}