using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("MedicalEvents")]
    public class MedicalEvent : AuditableEntity
    {
        public Guid PersonId { get; set; }

        public DateTime Date { get; set; }

        [Required] 
        public string Note { get; set; } = null!;

        public Person? Person { get; set; }
    }
}