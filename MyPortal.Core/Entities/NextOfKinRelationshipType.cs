using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    // Relationship of a staff member to their next of kin (Spouse, Partner, Sibling, …). A
    // dedicated adult-oriented lookup, distinct from the DfE CBDS RelationshipTypes used for
    // pupil contacts, which carries child-centric options (Childminder, Social Worker, …).
    [Table("NextOfKinRelationshipTypes")]
    public class NextOfKinRelationshipType : LookupEntity, ISystemEntity, IOrderedLookupEntity
    {
        public bool IsSystem { get; set; }

        public int DisplayOrder { get; set; }
    }
}
