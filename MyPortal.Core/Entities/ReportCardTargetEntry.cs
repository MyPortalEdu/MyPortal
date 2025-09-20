using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("ReportCardTargetEntries")]
    public class ReportCardTargetEntry : Entity
    {
        public Guid EntryId { get; set; }

        public Guid TargetId { get; set; }

        public bool IsCompleted { get; set; }

        public ReportCardEntry? Entry { get; set; }
        public ReportCardTarget? Target { get; set; }
    }
}