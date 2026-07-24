using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("ServiceTermSuperannuationSchemes")]
    public class ServiceTermSuperannuationScheme : Entity
    {
        public Guid ServiceTermId { get; set; }

        public Guid SuperannuationSchemeId { get; set; }

        // At most one per term, enforced by a filtered unique index.
        public bool IsMain { get; set; }

        public ServiceTerm? ServiceTerm { get; set; }
        public SuperannuationScheme? SuperannuationScheme { get; set; }
    }
}
