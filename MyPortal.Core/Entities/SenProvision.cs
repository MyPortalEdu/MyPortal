using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("SenProvisions")]
    public class SenProvision : Entity
    {
        public Guid StudentId { get; set; }

        public Guid SenProvisionTypeId { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        [Required]
        public string Note { get; set; } = null!;

        public Student? Student { get; set; }

        public SenProvisionType? SenProvisionType { get; set; }
    }
}