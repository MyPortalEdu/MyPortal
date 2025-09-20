using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("LogNoteTypes")]
    public class LogNoteType : LookupEntity
    {
        [Required]
        [StringLength(128)]
        public required string ColourCode { get; set; }

        [Required] 
        public required string IconClass { get; set; }
    }
}