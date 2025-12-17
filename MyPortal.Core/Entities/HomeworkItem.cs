using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    [Table("HomeworkItems")]
    public class HomeworkItem : Entity, IDirectoryEntity
    {
        public Guid DirectoryId { get; set; }

        [Required] 
        [StringLength(128)] 
        public string Title { get; set; } = null!;
        
        [StringLength(256)]
        public string? Description { get; set; }
    
        public bool SubmitOnline { get; set; }

        public int MaxPoints { get; set; }

        public Directory? Directory { get; set; }
    }
}