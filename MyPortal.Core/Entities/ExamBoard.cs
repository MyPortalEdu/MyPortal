using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("ExamBoards")]
    public class ExamBoard : Entity
    {
        // TODO: Populate Data

        [StringLength(20)]
        public string? Abbreviation { get; set; }
        
        [StringLength(128)]
        public string? FullName { get; set; }

        [StringLength(5)] 
        public string? Code { get; set; }

        public bool IsDomestic { get; set; }

        public bool UseEdi { get; set; }

        public bool IsActive { get; set; }
    }
}