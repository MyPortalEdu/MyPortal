using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    [Table("DiaryEvents")]
    public class DiaryEvent : AuditableEntity, ISystemEntity
    {
        public Guid EventTypeId { get; set; }

        public Guid? RoomId { get; set; }
        
        [Required, StringLength(256)]
        public required string Subject { get; set; }
        
        [StringLength(256)]
        public string? Description { get; set; }
        
        [StringLength(256)]
        public string? Location { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public bool IsAllDay { get; set; }

        /// <summary>
        /// Public events are visible to all users on the school diary
        /// </summary>
        public bool IsPublic { get; set; }

        public bool IsSystem { get; set; }
    }
}