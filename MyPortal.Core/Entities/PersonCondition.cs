using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("PersonConditions")]
    public class PersonCondition : Entity
    {
        public Guid PersonId { get; set; }

        public Guid MedicalConditionId { get; set; }

        // Whether this condition requires medication; gates the Medication detail.
        public bool RequiresMedication { get; set; }

        [StringLength(256)]
        public string? Medication { get; set; }

        // When the condition began / was diagnosed.
        public DateTime? StartDate { get; set; }

        // When the condition resolved (null = ongoing). A resolved condition is kept on record.
        public DateTime? EndDate { get; set; }

        // When the school was informed of the condition (SIMS "Info Received Date").
        public DateTime? InfoReceivedDate { get; set; }

        public string? Notes { get; set; }

        public Person? Person { get; set; }
        public MedicalCondition? MedicalCondition { get; set; }
    }
}
