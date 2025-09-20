using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("CommunicationLogs")]
    public class CommunicationLog : Entity
    {
        public Guid ContactId { get; set; }

        public Guid CommunicationTypeId { get; set; }

        public DateTime Date { get; set; }

        public string? Notes { get; set; }

        public bool IsOutgoing { get; set; }

        public Contact? Contact { get; set; }
        public CommunicationType? Type { get; set; }
    }
}