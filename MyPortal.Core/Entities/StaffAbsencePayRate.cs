using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    // The authorised rate an absence is paid at (SSP, occupational full/half pay, unpaid …).
    // Drives statutory sick-pay reporting and the payroll export.
    [Table("StaffAbsencePayRates")]
    public class StaffAbsencePayRate : LookupEntity, ISystemEntity, IOrderedLookupEntity
    {
        public bool IsSystem { get; set; }

        public int DisplayOrder { get; set; }
    }
}
