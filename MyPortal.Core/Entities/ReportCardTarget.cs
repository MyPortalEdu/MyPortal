using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("ReportCardTargets")]
    public class ReportCardTarget : Entity
    {
        public Guid ReportCardId { get; set; }

        public Guid TargetId { get; set; }

        public ReportCard? ReportCard { get; set; }
        public BehaviourTarget? Target { get; set; }
    }
}