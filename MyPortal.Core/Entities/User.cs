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
        
        // Identity
        [StringLength(256)]
        public string? Username { get; set; }

        [StringLength(256)]
        public string? NormalizedUsername { get; set; }

        [EmailAddress]
        [StringLength(256)]
        public string? Email { get; set; }

        [EmailAddress]
        [StringLength(256)]
        public string? NormalizedEmail { get; set; }

        public bool EmailConfirmed { get; set; }

        public string? PasswordHash { get; set; }

        public string? SecurityStamp { get; set; }

        public string? ConcurrencyStamp { get; set; }

        [Phone, StringLength(50)]
        public string? PhoneNumber { get; set; }

        public bool PhoneNumberConfirmed { get; set; }

        public bool TwoFactorEnabled { get; set; }

        public DateTime? LockoutEnd { get; set; }

        public bool LockoutEnabled { get; set; }

        public int AccessFailedCount { get; set; }
    }
}