using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("Languages")]
    public class Language : LookupEntity
    {
        public string? Code { get; set; }
    }
}