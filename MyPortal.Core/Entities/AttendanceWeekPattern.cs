using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("AttendanceWeekPatterns")]
    public class AttendanceWeekPattern : Entity
    {
        [Required, StringLength(128)]
        public required string Description { get; set; }
    }
}