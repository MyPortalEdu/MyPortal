using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    [Table("NextOfKinRelationshipTypes")]
    public class NextOfKinRelationshipType : LookupEntity, ISystemEntity
    {
        public bool IsSystem { get; set; }
    }
}