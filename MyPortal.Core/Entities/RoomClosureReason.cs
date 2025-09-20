using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    [Table("RoomClosureReasons")]
    public class RoomClosureReason : LookupEntity, ISystemEntity
    {
        public bool IsSystem { get; set; }
    }
}