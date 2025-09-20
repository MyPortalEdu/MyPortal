using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("BoarderStatus")]
    public class BoarderStatus : LookupEntity
    {
        public string? Code { get; set; }
    }
}