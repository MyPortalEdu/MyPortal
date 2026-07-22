using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    // A looked-after (in-care) episode. The current in-care status is derived from an open episode
    // (EndDate null). Replaces the old flat Student.InCare / CaringAuthorityId snapshot.
    [Table("StudentCareEpisodes")]
    public class StudentCareEpisode : Entity
    {
        public Guid StudentId { get; set; }

        // The local authority the child is looked-after by (mandatory for the School Census).
        public Guid CaringAuthorityId { get; set; }

        public Guid? LivingArrangementId { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [StringLength(1024)]
        public string? Comment { get; set; }

        public Student? Student { get; set; }
        public LocalAuthority? CaringAuthority { get; set; }
        public LivingArrangement? LivingArrangement { get; set; }
    }
}
