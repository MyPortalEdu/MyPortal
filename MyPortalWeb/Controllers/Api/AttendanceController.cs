using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPortal.Database.Enums;
using MyPortal.Logic.Attributes;
using MyPortal.Logic.Constants;
using MyPortal.Logic.Interfaces.Services;
using MyPortal.Logic.Models.Data.Attendance.Register;
using MyPortal.Logic.Models.Requests.Attendance;
using MyPortal.Logic.Models.Summary;
using MyPortalWeb.Models.Requests.Attendance;

namespace MyPortalWeb.Controllers.Api
{
    [Authorize]
    [Route("api/attendance")]
    public class AttendanceController : ControllerBase
    {
        private readonly IAttendanceService _attendanceService;

        public AttendanceController(IAttendanceService attendanceService)
        {
            _attendanceService = attendanceService;
        }

        [HttpPost]
        [Route("marks")]
        [Permission(PermissionValue.AttendanceEditAttendanceMarks)]
        [ProducesResponseType(200)]
        public async Task<IActionResult> SaveAttendanceMarks([FromBody] AttendanceMarkSummaryModel[] marks)
        {
            await _attendanceService.UpdateAttendanceMarks(marks);
            return Ok();
        }

        [HttpPost]
        [Route("registers/extraName")]
        [Permission(PermissionValue.AttendanceEditAttendanceMarks)]
        [ProducesResponseType(200)]
        public async Task<IActionResult> AddExtraName([FromBody] ExtraNameRequestModel model)
        {
            await _attendanceService.AddExtraName(model);

            return Ok();
        }

        [HttpDelete]
        [Route("registers/extraName/{extraNameId}")]
        [Permission(PermissionValue.AttendanceEditAttendanceMarks)]
        [ProducesResponseType(200)]
        public async Task<IActionResult> RemoveExtraName([FromRoute] Guid extraNameId)
        {
            await _attendanceService.RemoveExtraName(extraNameId);

            return Ok();
        }

        [HttpGet]
        [Route("registers")]
        [Authorize(Policy = Policies.UserType.Staff)]
        [Permission(PermissionValue.AttendanceViewAttendanceMarks)]
        [ProducesResponseType(typeof(IEnumerable<AttendanceRegisterSummaryModel>), 200)]
        public async Task<IActionResult> GetRegisters([FromQuery] RegisterSearchRequestModel searchOptions)
        {
            var registers = await _attendanceService.GetRegisters(searchOptions);

            return Ok(registers);
        }

        [HttpGet]
        [Route("registers/session")]
        [Authorize(Policy = Policies.UserType.Staff)]
        [Permission(PermissionValue.AttendanceEditAttendanceMarks)]
        [ProducesResponseType(typeof(AttendanceRegisterDataModel), 200)]
        public async Task<IActionResult> GetRegister([FromQuery] SessionRegisterRequestModel model)
        {
            var register =
                await _attendanceService.GetRegisterBySession(model.AttendanceWeekId, model.SessionId,
                    model.PeriodId);

            return Ok(register);
        }

        [HttpGet]
        [Route("registers/studentGroup")]
        [Authorize(Policy = Policies.UserType.Staff)]
        [Permission(PermissionValue.AttendanceEditAttendanceMarks)]
        [ProducesResponseType(typeof(AttendanceRegisterDataModel), 200)]
        public async Task<IActionResult> GetRegister([FromQuery] StudentGroupRegisterRequestModel model)
        {
            var register = await _attendanceService.GetRegisterByStudentGroup(model.StudentGroupId,
                model.AttendanceWeekId, model.PeriodId);

            return Ok(register);
        }

        [HttpGet]
        [Route("registers/custom")]
        [Authorize(Policy = Policies.UserType.Staff)]
        [Permission(PermissionValue.AttendanceEditAttendanceMarks)]
        [ProducesResponseType(typeof(AttendanceRegisterDataModel), 200)]
        public async Task<IActionResult> GetRegister([FromQuery] CustomRegisterRequestModel model)
        {
            var register =
                await _attendanceService.GetRegisterByDateRange(model.StudentGroupId, model.DateFrom, model.DateTo);

            return Ok(register);
        }
    }
}