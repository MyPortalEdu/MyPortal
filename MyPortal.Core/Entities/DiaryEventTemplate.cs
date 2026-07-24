using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Common.Enums;

namespace MyPortal.Core.Entities
{
    [Table("DiaryEventTemplates")]
    public class DiaryEventTemplate : LookupEntity
    {
        public DiaryEventKind Kind { get; set; }

        public int Minutes { get; set; }

        public int Hours { get; set; }

        public int Days { get; set; }
    }
}
