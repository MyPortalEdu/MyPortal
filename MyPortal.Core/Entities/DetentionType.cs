using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("DetentionTypes")]
    public class DetentionType : LookupEntity
    {
        public TimeSpan StartTime { get; set; }
        
        public TimeSpan EndTime { get; set; }
    }
}