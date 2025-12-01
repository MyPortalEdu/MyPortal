using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    [Table("People")]
    public class Person : AuditableEntity, IDirectoryEntity, ISoftDeleteEntity
    {
        public Guid DirectoryId { get; set; }
        
        [StringLength(128)]
        public string? Title { get; set; }
        
        [StringLength(256)]
        public string? PreferredFirstName { get; set; }
        
        [StringLength(256)]
        public string? PreferredLastName { get; set; }

        [Required]
        [StringLength(256)]
        public string FirstName { get; set; } = null!;
        
        [StringLength(256)]
        public string? MiddleName { get; set; }

        [Required]
        [StringLength(256)]
        public string LastName { get; set; } = null!;

        public Guid? PhotoId { get; set; }
        
        [StringLength(10)]
        public string? NhsNumber { get; set; }

        [Required]
        [StringLength(1)]
        public string Gender { get; set; } = null!;
        
        public DateTime? Dob { get; set; }
        
        public DateTime? Deceased { get; set; }

        public Guid? EthnicityId { get; set; }

        public bool IsDeleted { get; set; }

        public Photo? Photo { get; set; }
        public Ethnicity? Ethnicity { get; set; }
        public Directory? Directory { get; set; }
    }
}