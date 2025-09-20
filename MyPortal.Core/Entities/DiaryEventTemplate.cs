using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("DiaryEventTemplates")]
    public class DiaryEventTemplate : LookupEntity
    {
        public Guid DiaryEventTypeId { get; set; }

        public int Minutes { get; set; }

        public int Hours { get; set; }

        public int Days { get; set; }

        public DiaryEventType? DiaryEventType { get; set; }
    }
}