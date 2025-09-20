using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("ExamAssessmentModes")]
    public class ExamAssessmentMode : LookupEntity
    {
        // TODO: Populate Seed Data

        public bool ExternallyAssessed { get; set; }
    }
}