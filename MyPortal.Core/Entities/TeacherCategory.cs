using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    // What kind of teacher a staff member is (qualified, instructor, overseas-trained, …).
    // A census classification, required for School Census / workforce returns.
    [Table("TeacherCategories")]
    public class TeacherCategory : LookupEntity, IOrderedLookupEntity
    {
        // DfE code — populate from CBDS before using for a census return.
        [StringLength(10)]
        public string? Code { get; set; }

        public int DisplayOrder { get; set; }
    }
}
