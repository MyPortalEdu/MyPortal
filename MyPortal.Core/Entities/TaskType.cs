using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    [Table("TaskTypes")]
    public class TaskType : LookupEntity, ISystemEntity
    {
        public bool IsPersonal { get; set; }

        [Required] 
        public required string ColourCode { get; set; }

        public bool IsSystem { get; set; }
    }
}