using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("StudentAgentRelationships")]
    public class StudentAgentRelationship : Entity
    {
        public Guid StudentId { get; set; }

        public Guid AgentId { get; set; }

        public Guid RelationshipTypeId { get; set; }

        public Student? Student { get; set; }
        public Agent? Agent { get; set; }
        public RelationshipType? RelationshipType { get; set; }
    }
}