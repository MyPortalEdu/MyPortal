using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    [Table("StaffAbsenceTypes")]
    public class StaffAbsenceType : LookupEntity, ISystemEntity
    {
        public bool IsSystem { get; set; }

        public bool IsAuthorised { get; set; }
    }
}