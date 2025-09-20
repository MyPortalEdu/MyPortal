using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    [Table("ExamQualificationLevels")]
    public class ExamQualificationLevel : LookupEntity, ISystemEntity
    {
        // TODO: Populate Seed Data

        public Guid QualificationId { get; set; }

        public Guid? DefaultGradeSetId { get; set; }

        [StringLength(25)] 
        public string? JcLevelCode { get; set; }

        public bool IsSystem { get; set; }

        public GradeSet? DefaultGradeSet { get; set; }
        public ExamQualification? Qualification { get; set; }
    }
}