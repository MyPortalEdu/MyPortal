using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("SenTypes")]
    public class SenType : LookupEntity
    {
        public string? Code { get; set; }
    }
}