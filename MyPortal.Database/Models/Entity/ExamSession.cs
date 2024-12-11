using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Database.BaseTypes;

namespace MyPortal.Database.Models.Entity
{
    [Table("exam_session")]
    public class ExamSession : LookupItem
    {
        // TODO: Populate Data

        [Column(Order = 4)] public TimeSpan StartTime { get; set; }

        public virtual ICollection<ExamDate> ExamDates { get; set; }
    }
}