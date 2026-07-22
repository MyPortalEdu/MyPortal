using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    // The child's living arrangement while looked-after (e.g. foster care, residential care).
    [Table("LivingArrangements")]
    public class LivingArrangement : LookupEntity
    {
    }
}
