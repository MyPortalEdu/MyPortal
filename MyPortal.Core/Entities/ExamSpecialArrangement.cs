using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    [Table("ExamSpecialArrangements")]
    public class ExamSpecialArrangement : Entity, ISystemEntity
    {
        [Required]
        public required string Description { get; set; }

        public bool IsSystem { get; set; }
    }
}