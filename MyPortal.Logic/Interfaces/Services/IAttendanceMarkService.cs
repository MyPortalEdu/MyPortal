﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MyPortal.Logic.Models.DataGrid;
using MyPortal.Logic.Models.Entity;
using MyPortal.Logic.Models.Requests.Attendance;

namespace MyPortal.Logic.Interfaces.Services
{
    public interface IAttendanceMarkService : IService
    {
        Task<AttendanceSummary> GetAttendanceSummaryByStudent(Guid studentId, Guid academicYearId);
        Task<AttendanceMarkModel> GetAttendanceMark(Guid studentId, Guid attendanceWeekId, Guid periodId, bool returnNoMark = false);
        Task<IEnumerable<AttendanceRegisterStudentModel>> GetAttendanceMarksBySession(Guid attendanceWeekId,
            Guid sessionId);
        Task Save(params AttendanceMarkListModel[] marks);
        Task Delete(params Guid[] attendanceMarkIds);
    }
}