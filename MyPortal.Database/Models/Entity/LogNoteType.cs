using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using MyPortal.Database.BaseTypes;

namespace MyPortal.Database.Models.Entity
{
    [DataContract]
    [Table("LogNoteTypes")]
    public class LogNoteType : LookupItem
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage",
            "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public LogNoteType()
        {
            LogNotes = new HashSet<LogNote>();
        }

        [Required]
        [Column(Order = 4)]
        [StringLength(128)]
        public string ColourCode { get; set; }

        [Column(Order = 5)] [Required] public string IconClass { get; set; }


        public virtual ICollection<LogNote> LogNotes { get; set; }
    }
}