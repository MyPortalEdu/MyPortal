using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    [Table("ExamSpecialArrangements")]
    public class ExamSpecialArrangement : Entity, ISystemEntity
    {
        [Required]
        public string Description { get; set; } = null!;

        public bool IsSystem { get; set; }
    }
}