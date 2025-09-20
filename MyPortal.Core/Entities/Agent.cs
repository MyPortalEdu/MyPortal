using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    [Table("Agents")]
    public class Agent : Entity, ISoftDeleteEntity
    {
        public Guid AgencyId { get; set; }

        public Guid PersonId { get; set; }

        public Guid AgentTypeId { get; set; }

        
        [StringLength(128)]
        public string? JobTitle { get; set; }

        public bool IsDeleted { get; set; }

        public Agency? Agency { get; set; }
        public AgentType? AgentType { get; set; }
        public Person? Person { get; set; }
    }
}