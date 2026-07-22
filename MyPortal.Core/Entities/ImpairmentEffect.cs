using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    [Table("ImpairmentEffects")]
    public class ImpairmentEffect : LookupEntity, ISystemEntity, IOrderedLookupEntity
    {
        public bool IsSystem { get; set; }

        public int DisplayOrder { get; set; }
    }
}
