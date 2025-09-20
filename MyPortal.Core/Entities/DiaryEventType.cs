using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    [Table("DiaryEventTypes")]
    public class DiaryEventType : LookupEntity, ISystemEntity
    {
        [StringLength(128)]
        public required string ColourCode { get; set; }

        public bool IsSystem { get; set; }
    }
}