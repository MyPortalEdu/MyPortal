using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    [Table("AttendanceCodes")]
    public class AttendanceCode : Entity, ISystemEntity
    {
        [Required, StringLength(1)] 
        public string Code { get; set; } = null!;

        [Required, StringLength(128)] 
        public string Description { get; set; } = null!;

        public Guid AttendanceCodeTypeId { get; set; }

        public bool IsActive { get; set; }

        // Only users with the UseRestrictedCodes permission can use these
        public bool IsRestricted { get; set; }

        public bool IsSystem { get; set; }

        public AttendanceCodeType? CodeType { get; set; }
    }
}