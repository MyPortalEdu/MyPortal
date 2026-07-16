using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    [Table("ExamQualifications")]
    public class ExamQualification : LookupEntity, ISystemEntity
    {
        // TODO: Populate Seed Data

        public string? JcQualificationCode { get; set; }

        // Ofqual/QWS 8-digit Qualification Number (QN), distinct from the JCQ code.
        [StringLength(8)]
        public string? QualificationNumber { get; set; }

        public bool IsSystem { get; set; }
    }
}