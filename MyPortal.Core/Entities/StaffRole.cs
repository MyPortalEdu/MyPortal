using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    // DfE School Workforce Census staff role (CBDS CS050) — e.g. Classroom
    // Teacher (TCHR), Head Teacher (HDTR), Teaching Assistant (TASS).
    [Table("StaffRoles")]
    public class StaffRole : LookupEntity, IOrderedLookupEntity
    {
        [StringLength(10)]
        public string? Code { get; set; }

        public int DisplayOrder { get; set; }
    }
}
