using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("ExamSessions")]
    public class ExamSession : LookupEntity
    {
        // TODO: Populate Seed Data

        public TimeSpan StartTime { get; set; }
    }
}