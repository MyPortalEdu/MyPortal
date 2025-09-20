using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("Users")]
    public class User : Entity
    {
        public DateTime CreatedAt { get; set; }

        public Guid? PersonId { get; set; }
        
        public int UserType { get; set; }

        public bool IsEnabled { get; set; }

        public Person? Person { get; set; }
    }
}