using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("PersonConditions")]
    public class PersonCondition : Entity
    {
        public Guid PersonId { get; set; }

        public Guid MedicalConditionId { get; set; }

        public bool MedicationTaken { get; set; }
        
        [StringLength(256)]
        public string? Medication { get; set; }

        public Person? Person { get; set; }
        public MedicalCondition? MedicalCondition { get; set; }
    }
}