using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    [Table("BehaviourOutcomes")]
    public class BehaviourOutcome : LookupEntity, ISystemEntity
    {
        public bool IsSystem { get; set; }
    }
}