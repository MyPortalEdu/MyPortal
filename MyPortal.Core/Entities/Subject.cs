using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    [Table("Subjects")]
    public class Subject : Entity, ISoftDeleteEntity
    {
        public Guid SubjectCodeId { get; set; }
        
        [Required]
        [StringLength(256)]
        public required string Name { get; set; }
        
        [Required]
        [StringLength(5)]
        public required string Code { get; set; }

        public bool IsDeleted { get; set; }

        public SubjectCode? SubjectCode { get; set; }
    }
}