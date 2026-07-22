using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    // Deliberately alongside HasQts, not replacing it: the flag answers "is this person qualified",
    // the code answers "under which classification", and the census needs the latter.
    [Table("TeacherStatuses")]
    public class TeacherStatus : LookupEntity, IOrderedLookupEntity
    {
        // Seeded null — populate from CBDS before using for a census return.
        [StringLength(10)]
        public string? Code { get; set; }

        public int DisplayOrder { get; set; }
    }
}
