using System;
using MyPortal.Database.Models.Entity;
using MyPortal.Logic.Models.Data.Curriculum;

namespace MyPortal.Logic.Models.Data.Attendance
{
    public class AttendanceWeekModel
    {
        public AttendanceWeekModel(AttendanceWeek model)
        {
            LoadFromModel(model);
        }

        private void LoadFromModel(AttendanceWeek model)
        {
            WeekPatternId = model.WeekPatternId;
            AcademicTermId = model.AcademicTermId;
            Beginning = model.Beginning;
            IsNonTimetable = model.IsNonTimetable;

            if (model.WeekPattern != null)
            {
                WeekPattern = new AttendanceWeekPatternModel(model.WeekPattern);
            }

            if (model.AcademicTerm != null)
            {
                AcademicTerm = new AcademicTermModel(model.AcademicTerm);
            }
        }

        public Guid WeekPatternId { get; set; }

        public Guid AcademicTermId { get; set; }

        public DateTime Beginning { get; set; }

        public bool IsNonTimetable { get; set; }

        public AttendanceWeekPatternModel WeekPattern { get; set; }
        public AcademicTermModel AcademicTerm { get; set; }
    }
}