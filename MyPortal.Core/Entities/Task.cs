using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    [Table("Tasks")]
    public class Task : AuditableEntity, ISystemEntity
    {
        public Guid TypeId { get; set; }

        public Guid? AssignedToId { get; set; }

        public DateTime? DueDate { get; set; }

        public DateTime? CompletedDate { get; set; }

        [Required, StringLength(128)] 
        public string Title { get; set; } = null!;
        
        [StringLength(256)]
        public string? Description { get; set; }

        public bool Completed { get; set; }

        public bool CanAssigneeEdit { get; set; }

        public bool IsSystem { get; set; }

        public Person? AssignedTo { get; set; }
        public TaskType? Type { get; set; }
    }
}