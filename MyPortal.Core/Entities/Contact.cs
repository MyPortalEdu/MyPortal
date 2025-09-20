using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("Contacts")]
    public class Contact : Entity
    {
        public Guid PersonId { get; set; }

        public bool ParentalBallot { get; set; }
        
        [StringLength(256)]
        public string? PlaceOfWork { get; set; }
        
        [StringLength(256)]
        public string? JobTitle { get; set; }
        
        [StringLength(128)]
        public string? NiNumber { get; set; }

        public Person? Person { get; set; }
    }
}