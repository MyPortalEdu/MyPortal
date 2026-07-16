using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    [Table("RelationshipTypes")]
    public class RelationshipType : LookupEntity, IOrderedLookupEntity
    {
        // DfE CBDS Relationship code (e.g. PAM, PAF, CAR).
        [StringLength(10)]
        public string? Code { get; set; }

        public int DisplayOrder { get; set; }
    }
}
