using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    [Table("YearGroups")]
    public class YearGroup : Entity, IStudentGroupEntity
    {
        public Guid StudentGroupId { get; set; }

        public Guid CurriculumYearGroupId { get; set; }

        public StudentGroup? StudentGroup { get; set; }
        public CurriculumYearGroup? CurriculumYearGroup { get; set; }
    }
}