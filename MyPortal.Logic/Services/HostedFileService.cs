using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MyPortal.Database.Interfaces.Repositories;
using MyPortal.Logic.Exceptions;
using MyPortal.Logic.Interfaces;
using MyPortal.Logic.Interfaces.Services;
using MyPortal.Logic.Models.Web;

namespace MyPortal.Logic.Services
{
    public sealed class HostedFileService : BaseService, IFileService
    {
        private readonly IHostedFileProvider _fileProvider;
        private readonly IDocumentAccessController _documentAccessController;

        public HostedFileService(ISessionUser sessionUser, IHostedFileProviderFactory fileProviderFactory,
            IDocumentAccessController documentAccessController) : base(
            sessionUser)
        {
            _fileProvider = fileProviderFactory.CreateHostedFileProvider();
            _documentAccessController = documentAccessController;
        }

        public async Task AttachFileToDocument(Guid documentId, string fileId)
        {
            await _documentAccessController.VerifyDocumentAccess(documentId, true);
            
            await using var unitOfWork = await User.GetConnection();

            var document = await unitOfWork.GetRepository<IDocumentRepository>().GetById(documentId);

            if (document == null)
            {
                throw new NotFoundException("Document not found.");
            }

            var file = await _fileProvider.CreateFileFromId(fileId);

            document.Attachment = file;

            await unitOfWork.GetRepository<IDocumentRepository>().Update(document);

            await unitOfWork.SaveChangesAsync();
        }

        public async Task RemoveFileFromDocument(Guid documentId)
        {
            await _documentAccessController.VerifyDocumentAccess(documentId, true);
            
            await using var unitOfWork = await User.GetConnection();

            var file = await unitOfWork.GetRepository<IFileRepository>().GetByDocumentId(documentId);

            if (file == null)
            {
                throw new LogicException("No file is attached to this document.");
            }

            await unitOfWork.GetRepository<IFileRepository>().Delete(file.Id);

            await unitOfWork.SaveChangesAsync();
        }

        public async Task<IEnumerable<WebAction>> GetWebActionsByDocument(Guid documentId)
        {
            await _documentAccessController.VerifyDocumentAccess(documentId, false);
            
            await using var unitOfWork = await User.GetConnection();

            var file = await unitOfWork.GetRepository<IFileRepository>().GetByDocumentId(documentId);

            var webActions = await _fileProvider.GetWebActions(file.FileId);

            return webActions;
        }
    }
}