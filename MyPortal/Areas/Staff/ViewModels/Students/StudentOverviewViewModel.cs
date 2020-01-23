﻿using System.Collections.Generic;
using MyPortal.BusinessLogic.Dtos;

namespace MyPortal.Areas.Staff.ViewModels.Students
{
    public class StudentOverviewViewModel
    {
        public StudentDto Student { get; set; }
        public IDictionary<int, string> LogTypes { get; set; }
        public ProfileLogNoteDto LogNote { get; set; }
        public PersonAttachmentDto Attachment { get; set; } 
        public IDictionary<int, string> CommentBanks { get; set; }
        public bool HasAttendanceData { get; set; }
        public double? Attendance { get; set; }
        public int? AchievementCount { get; set; }
        public int? BehaviourCount { get; set; }
    }
}