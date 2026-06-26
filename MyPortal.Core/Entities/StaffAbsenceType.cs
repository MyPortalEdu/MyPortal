using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    [Table("StaffAbsenceTypes")]
    public class StaffAbsenceType : LookupEntity, ISystemEntity, IOrderedLookupEntity
    {
        public bool IsSystem { get; set; }

        public bool IsAuthorised { get; set; }

        // DfE CBDS Absence Category code (e.g. SIC, MAT, SEC) — for workforce-census mapping.
        [StringLength(10)]
        public string? Code { get; set; }

        public int DisplayOrder { get; set; }
    }
}
