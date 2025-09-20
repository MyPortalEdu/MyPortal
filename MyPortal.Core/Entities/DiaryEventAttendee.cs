using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("DiaryEventAttendees")]
    public class DiaryEventAttendee : Entity
    {
        public Guid EventId { get; set; }

        public Guid PersonId { get; set; }

        public Guid? ResponseId { get; set; }

        public bool IsRequired { get; set; }

        public bool? HasAttended { get; set; }

        public bool CanEditEvent { get; set; }

        public DiaryEvent? Event { get; set; }
        public Person? Person { get; set; }
        public DiaryEventAttendeeResponse? Response { get; set; }
    }
}