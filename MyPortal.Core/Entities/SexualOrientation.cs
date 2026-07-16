using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    [Table("SexualOrientations")]
    public class SexualOrientation : LookupEntity, IOrderedLookupEntity
    {
        public int DisplayOrder { get; set; }
    }
}
