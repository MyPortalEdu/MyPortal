using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Common.Enums;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    [Table("DiaryEvents")]
    public class DiaryEvent : Entity, IAuditableEntity, ISystemEntity, IVersionedEntity
    {
        // Only User is user-created; other kinds are reserved for system events (detentions, cover, holidays, training).
        public DiaryEventKind Kind { get; set; }

        public Guid? RoomId { get; set; }

        [Required, StringLength(256)]
        public string Subject { get; set; } = null!;
        
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
        
        public Guid CreatedById { get; set; }
        public string CreatedByIpAddress { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public Guid LastModifiedById { get; set; }
        public string LastModifiedByIpAddress { get; set; } = string.Empty;
        public DateTime LastModifiedAt { get; set; }
        public User? CreatedBy { get; set; }
        public User? LastModifiedBy { get; set; }
        public long Version { get; set; }
    }
}