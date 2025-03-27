using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPortal.Database.Enums;
using MyPortal.Logic.Attributes;
using MyPortal.Logic.Constants;
using MyPortal.Logic.Interfaces.Services;
using MyPortal.Logic.Models.Data.Students;
using MyPortal.Logic.Models.Requests.Student.LogNotes;
using MyPortalWeb.Controllers.BaseControllers;

namespace MyPortalWeb.Controllers.Api
{
    [Authorize]
    public class LogNotesController : BaseApiController
    {
        private readonly ILogNoteService _logNoteService;
        private readonly IAcademicYearService _academicYearService;

        public LogNotesController(ILogNoteService logNoteService, IAcademicYearService academicYearService)
        {
            _academicYearService = academicYearService;
            _logNoteService = logNoteService;
        }

        [HttpGet]
        [Route("api/logNotes/{logNoteId}")]
        [Permission(PermissionValue.StudentViewStudentLogNotes)]
        [ProducesResponseType(typeof(LogNoteModel), 200)]
        public async Task<IActionResult> GetById([FromQuery] Guid logNoteId)
        {
            var logNote = await _logNoteService.GetLogNoteById(logNoteId);

            return Ok(logNote);
        }

        [HttpGet]
        [Route("api/logNotes/types")]
        [ProducesResponseType(typeof(IEnumerable<LogNoteTypeModel>), 200)]
        public async Task<IActionResult> GetTypes()
        {
            var logNoteTypes = await _logNoteService.GetLogNoteTypes();

            return Ok(logNoteTypes);
        }

        [HttpGet]
        [Route("api/students/{studentId}/logNotes")]
        [Permission(PermissionValue.StudentViewStudentLogNotes)]
        [ProducesResponseType(typeof(IEnumerable<LogNoteModel>), 200)]
        public async Task<IActionResult> GetByStudent([FromRoute] Guid studentId, [FromQuery] Guid? academicYearId)
        {
            if (academicYearId == null || academicYearId == Guid.Empty)
            {
                academicYearId = (await _academicYearService.GetCurrentAcademicYear(true))?.Id;
            }

            if (academicYearId.HasValue)
            {
                var logNotes =
                    await _logNoteService.GetLogNotesByStudent(studentId, academicYearId.Value);

                var result = logNotes;

                return Ok(result);
            }

            return Error(HttpStatusCode.BadRequest, "No academic year found.");
        }

        [HttpPost]
        [Authorize(Policy = Policies.UserType.Staff)]
        [Permission(PermissionValue.StudentEditStudentLogNotes)]
        [Route("api/logNotes")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> Create([FromBody] LogNoteRequestModel requestModel)
        {
            await _logNoteService.CreateLogNote(requestModel);

            return Ok();
        }

        [HttpPut]
        [Authorize(Policy = Policies.UserType.Staff)]
        [Permission(PermissionValue.StudentEditStudentLogNotes)]
        [Route("api/logNotes/{logNoteId}")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> Update([FromRoute] Guid logNoteId, [FromBody] LogNoteRequestModel requestModel)
        {
            await _logNoteService.UpdateLogNote(logNoteId, requestModel);

            return Ok();
        }

        [HttpDelete]
        [Authorize(Policy = Policies.UserType.Staff)]
        [Permission(PermissionValue.StudentEditStudentLogNotes)]
        [Route("api/logNotes/{logNoteId}")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> Delete([FromRoute] Guid logNoteId)
        {
            await _logNoteService.DeleteLogNote(logNoteId);

            return Ok();
        }
    }
}