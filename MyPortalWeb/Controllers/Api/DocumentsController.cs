using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPortal.Logic.Interfaces.Services;
using MyPortal.Logic.Models.Data.Documents;
using MyPortal.Logic.Models.Requests.Documents;
using MyPortal.Logic.Models.Summary;
using MyPortal.Logic.Models.Web;
using MyPortal.Logic.Services;
using MyPortalWeb.Controllers.BaseControllers;

namespace MyPortalWeb.Controllers.Api
{
    [Authorize]
    [Route("api/documents")]
    public class DocumentsController : BaseApiController
    {
        private readonly IDocumentService _documentService;
        private readonly IFileService _fileService;

        public DocumentsController(IDocumentService documentService, IFileService fileService)
        {
            _documentService = documentService;
            _fileService = fileService;
        }

        [HttpGet]
        [Route("{documentId}")]
        [ProducesResponseType(typeof(DocumentModel), 200)]
        public async Task<IActionResult> GetDocumentById([FromRoute] Guid documentId)
        {
            var document = await _documentService.GetDocumentById(documentId);

            return Ok(document);
        }

        [HttpPost]
        [Route("")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> CreateDocument([FromBody] DocumentRequestModel model)
        {
            await _documentService.CreateDocument(model);

            return Ok();
        }

        [HttpPost]
        [Route("{documentId}/upload-file")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> UploadFile([FromRoute] Guid documentId,
            [FromBody] FileUploadRequestModel requestModel)
        {
            if (_fileService is not LocalFileService localFileService)
            {
                return Error(HttpStatusCode.NotImplemented, "Direct file upload is not supported in this configuration.");
            }

            requestModel.DocumentId = documentId;

            await localFileService.UploadFileToDocument(requestModel);

            return Ok();
        }

        [HttpPost]
        [Route("{documentId}/link-hosted-file")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> LinkHostedFile([FromBody] HostedFileRequestModel requestModel)
        {
            if (_fileService is not HostedFileService hostedFileService)
            {
                return Error(HttpStatusCode.NotImplemented, "Hosted file linking is not supported in this configuration.");
            }

            await hostedFileService.AttachFileToDocument(requestModel.DocumentId, requestModel.FileId);

            return Ok();
        }

        [HttpDelete]
        [Route("{documentId}/file")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> RemoveAttachment([FromRoute] Guid documentId)
        {
            await _fileService.RemoveFileFromDocument(documentId);

            return Ok();
        }

        [HttpPut]
        [Route("{documentId}")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> UpdateDocument([FromRoute] Guid documentId,
            [FromBody] DocumentRequestModel model)
        {
            await _documentService.UpdateDocument(documentId, model);

            return Ok();
        }

        [HttpDelete]
        [Route("{documentId}")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> DeleteDocument([FromRoute] Guid documentId)
        {
            await _documentService.DeleteDocument(documentId);

            return Ok();
        }

        [HttpGet]
        [Route("{documentId}/webActions")]
        [ProducesResponseType(typeof(IEnumerable<WebAction>), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GetWebActions([FromQuery] Guid documentId)
        {
            if (_fileService is HostedFileService hostedFileService)
            {
                var webActions = await hostedFileService.GetWebActionsByDocument(documentId);

                return Ok(webActions);
            }

            return Ok(new List<WebAction>());
        }

        [HttpGet]
        [Route("directories/{directoryId}/children")]
        [ProducesResponseType(typeof(DirectoryChildWrapper), 200)]
        public async Task<IActionResult> GetDirectoryChildren([FromRoute] Guid directoryId)
        {
            var directory = await _documentService.GetDirectoryById(directoryId);

            var children =
                await _documentService.GetDirectoryChildren(directoryId);

            var childList = new List<DirectoryChildSummaryModel>();

            childList.AddRange(children.Subdirectories.Select(x => x.GetListModel()));
            childList.AddRange(children.Files.Select(x => x.GetListModel()));

            var response = new DirectoryChildWrapper
            {
                Directory = directory,
                Children = childList
            };

            return Ok(response);
        }

        [HttpPost]
        [Route("directories")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> CreateDirectory([FromBody] DirectoryRequestModel requestModel)
        {
            await _documentService.CreateDirectory(requestModel);

            return Ok();
        }

        [HttpPut]
        [Route("directories/{directoryId}")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> UpdateDirectory([FromRoute] Guid directoryId,
            [FromBody] DirectoryRequestModel requestModel)
        {
            await _documentService.UpdateDirectory(directoryId, requestModel);

            return Ok();
        }

        [HttpDelete]
        [Route("directories/{directoryId}")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> DeleteDirectory([FromRoute] Guid directoryId)
        {
            await _documentService.DeleteDirectory(directoryId);

            return Ok();
        }

        [HttpGet]
        [Route("directories/{directoryId}")]
        [ProducesResponseType(typeof(DirectoryModel), 200)]
        public async Task<IActionResult> GetDirectoryById([FromRoute] Guid directoryId)
        {
            var directory = await _documentService.GetDirectoryById(directoryId);

            return Ok(directory);
        }
    }
}