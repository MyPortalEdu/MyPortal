using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Common.Enums;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    [Table("DiaryEventTypes")]
    public class DiaryEventType : LookupEntity, ISystemEntity
    {
        [StringLength(128)]
        public string ColourCode { get; set; } = null!;

        public bool IsSystem { get; set; }

        public DiaryEventKind Kind { get; set; }
    }
}