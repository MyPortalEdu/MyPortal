using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    // Adult-oriented, deliberately separate from the CBDS RelationshipTypes used for pupil
    // contacts — those are child-centric (Childminder, Social Worker, …).
    [Table("NextOfKinRelationshipTypes")]
    public class NextOfKinRelationshipType : LookupEntity, ISystemEntity, IOrderedLookupEntity
    {
        public bool IsSystem { get; set; }

        public int DisplayOrder { get; set; }
    }
}
