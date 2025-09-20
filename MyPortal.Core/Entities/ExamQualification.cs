using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    [Table("ExamQualifications")]
    public class ExamQualification : LookupEntity, ISystemEntity
    {
        // TODO: Populate Seed Data

        public string? JcQualificationCode { get; set; }

        public bool IsSystem { get; set; }
    }
}