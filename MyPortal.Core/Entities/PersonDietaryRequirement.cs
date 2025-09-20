using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("PersonDietaryRequirements")]
    public class PersonDietaryRequirement : Entity
    {
        public Guid PersonId { get; set; }

        public Guid DietaryRequirementId { get; set; }

        public DietaryRequirement? DietaryRequirement { get; set; }
        public Person? Person { get; set; }
    }
}