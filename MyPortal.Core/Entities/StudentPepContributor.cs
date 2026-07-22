using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    // A person who contributes to a Personal Education Plan (e.g. the designated teacher).
    [Table("StudentPepContributors")]
    public class StudentPepContributor : Entity
    {
        public Guid StudentPepId { get; set; }

        public Guid PersonId { get; set; }

        public StudentPep? StudentPep { get; set; }
        public Person? Person { get; set; }
    }
}
