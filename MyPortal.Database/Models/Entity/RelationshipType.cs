using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Database.BaseTypes;

namespace MyPortal.Database.Models.Entity
{
    [Table("relationship_type")]
    public class RelationshipType : LookupItem
    {
        public RelationshipType()
        {
            StudentAgents = new HashSet<StudentAgentRelationship>();
            StudentContacts = new HashSet<StudentContactRelationship>();
        }

        public ICollection<StudentAgentRelationship> StudentAgents { get; set; }
        public ICollection<StudentContactRelationship> StudentContacts { get; set; }
    }
}