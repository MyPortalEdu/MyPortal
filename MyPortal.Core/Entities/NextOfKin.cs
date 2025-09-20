using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("NextOfKin")]
    public class NextOfKin : Entity
    {
        public Guid StaffMemberId { get; set; }

        public Guid NextOfKinPersonId { get; set; }

        public Guid RelationshipTypeId { get; set; }

        public StaffMember? StaffMember { get; set; }
        public Person? NextOfKinPerson { get; set; }
        public NextOfKinRelationshipType? RelationshipType { get; set; }
    }
}