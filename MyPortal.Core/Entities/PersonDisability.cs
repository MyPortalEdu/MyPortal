using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("PersonDisabilities")]
    public class PersonDisability : Entity
    {
        public Guid PersonId { get; set; }

        public Guid DisabilityId { get; set; }

        public Disability? Disability { get; set; }
        public Person? Person { get; set; }
    }
}
