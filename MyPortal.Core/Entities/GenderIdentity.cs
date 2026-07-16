using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    [Table("GenderIdentities")]
    public class GenderIdentity : LookupEntity, IOrderedLookupEntity
    {
        public int DisplayOrder { get; set; }
    }
}
