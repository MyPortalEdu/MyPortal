using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("LogNoteTypes")]
    public class LogNoteType : LookupEntity
    {
        [Required]
        [StringLength(128)] 
        public string ColourCode { get; set; } = null!;

        [Required]
        public string IconClass { get; set; } = null!;
    }
}