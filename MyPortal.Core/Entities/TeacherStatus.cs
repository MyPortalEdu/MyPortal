using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    // Coded qualified-teacher status for census returns. Sits alongside the HasQts flag rather
    // than replacing it: the flag answers "is this person qualified", the code answers "under
    // which classification", and the census needs the latter.
    [Table("TeacherStatuses")]
    public class TeacherStatus : LookupEntity, IOrderedLookupEntity
    {
        // DfE code — populate from CBDS before using for a census return.
        [StringLength(10)]
        public string? Code { get; set; }

        public int DisplayOrder { get; set; }
    }
}
