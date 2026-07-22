using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    // The Equality Act determinant: whether a person's impairment has a substantial and long-term
    // adverse effect on their ability to carry out normal day-to-day activities.
    [Table("ImpairmentEffects")]
    public class ImpairmentEffect : LookupEntity, ISystemEntity, IOrderedLookupEntity
    {
        public bool IsSystem { get; set; }

        public int DisplayOrder { get; set; }
    }
}
