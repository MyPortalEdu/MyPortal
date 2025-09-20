using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("Detentions")]
    public class Detention : Entity
    {
        public Guid DetentionTypeId { get; set; }

        public Guid EventId { get; set; }

        public Guid? SupervisorId { get; set; }

        public DetentionType? Type { get; set; }
        public DiaryEvent? Event { get; set; }
        public StaffMember? Supervisor { get; set; }
    }
}