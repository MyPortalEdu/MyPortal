using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    // The payroll-facing category an absence is reported under, for the payroll provider export.
    // Distinct from the operational absence type.
    [Table("StaffAbsencePayrollReasons")]
    public class StaffAbsencePayrollReason : LookupEntity, ISystemEntity, IOrderedLookupEntity
    {
        public bool IsSystem { get; set; }

        public int DisplayOrder { get; set; }
    }
}
