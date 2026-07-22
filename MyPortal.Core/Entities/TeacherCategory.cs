using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    [Table("TeacherCategories")]
    public class TeacherCategory : LookupEntity, IOrderedLookupEntity
    {
        // Seeded null — populate from CBDS before using for a census return.
        [StringLength(10)]
        public string? Code { get; set; }

        public int DisplayOrder { get; set; }
    }
}
