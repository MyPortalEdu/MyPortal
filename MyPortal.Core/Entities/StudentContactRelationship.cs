using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("StudentContactRelationships")]
    public class StudentContactRelationship : Entity
    {
        public Guid RelationshipTypeId { get; set; }

        public Guid StudentId { get; set; }

        public Guid ContactId { get; set; }

        public bool HasCorrespondence { get; set; }

        public bool HasParentalResponsibility { get; set; }

        public bool HasPupilReport { get; set; }

        public bool HasCourtOrder { get; set; }

        public RelationshipType? RelationshipType { get; set; }
        public Student? Student { get; set; }
        public Contact? Contact { get; set; }
    }
}