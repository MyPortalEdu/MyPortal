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
        public string Name { get; set; } = null!;

        [Required]
        [StringLength(5)]
        public string Code { get; set; } = null!;

        public bool IsDeleted { get; set; }

        public SubjectCode? SubjectCode { get; set; }
    }
}