using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    // Route by which a teacher gained Qualified Teacher Status (CBDS CS069) —
    // e.g. School Direct (SCD), Teach First (TFST), Overseas Trained (OTTP).
    [Table("QtsRoutes")]
    public class QtsRoute : LookupEntity, IOrderedLookupEntity
    {
        [StringLength(10)]
        public string? Code { get; set; }

        public int DisplayOrder { get; set; }
    }
}
