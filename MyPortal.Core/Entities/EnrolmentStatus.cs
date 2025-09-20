using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("EnrolmentStatus")]
    public class EnrolmentStatus : LookupEntity
    {
        public string? Code { get; set; }
    }
}