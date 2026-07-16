using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    [Table("MedicalEvents")]
    public class MedicalEvent : Entity, IAuditableEntity, IVersionedEntity
    {
        public Guid PersonId { get; set; }

        public DateTime Date { get; set; }

        [Required] 
        public string Note { get; set; } = null!;

        public Person? Person { get; set; }
        
        public Guid CreatedById { get; set; }
        public string CreatedByIpAddress { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public Guid LastModifiedById { get; set; }
        public string LastModifiedByIpAddress { get; set; } = string.Empty;
        public DateTime LastModifiedAt { get; set; }
        public User? CreatedBy { get; set; }
        public User? LastModifiedBy { get; set; }
        public long Version { get; set; }
    }
}