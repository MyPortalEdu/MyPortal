using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("ResultSets")]
    public class ResultSet : LookupEntity
    {
        [Required]
        [StringLength(256)]
        public required string Name { get; set; }

        public bool IsLocked { get; set; }
    }
}