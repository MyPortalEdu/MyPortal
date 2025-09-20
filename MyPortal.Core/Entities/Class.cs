using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    [Table("Classes")]
    public class Class : Entity, IDirectoryEntity
    {
        public Guid CourseId { get; set; }

        public Guid CurriculumGroupId { get; set; }

        public Guid DirectoryId { get; set; }
        
        [Required, StringLength(10)]
        public required string Code { get; set; }

        public Course? Course { get; set; }
        public CurriculumGroup? Group { get; set; }
        public Directory? Directory { get; set; }
    }
}