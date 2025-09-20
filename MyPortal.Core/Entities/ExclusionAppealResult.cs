using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    [Table("ExclusionAppealResults")]
    public class ExclusionAppealResult : LookupEntity, ISystemEntity
    {
        public bool IsSystem { get; set; }
    }
}