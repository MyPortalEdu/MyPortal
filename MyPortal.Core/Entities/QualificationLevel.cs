using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("QualificationLevels")]
    public class QualificationLevel : LookupEntity
    {
        public int? OfqualLevel { get; set; }
    }
}
