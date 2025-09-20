using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("BehaviourStatus")]
    public class BehaviourStatus : LookupEntity
    {
        public bool IsResolved { get; set; }
    }
}