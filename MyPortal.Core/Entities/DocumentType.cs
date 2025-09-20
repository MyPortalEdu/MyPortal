using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    [Table("DocumentTypes")]
    public class DocumentType : LookupEntity, ISystemEntity
    {
        public bool Staff { get; set; }

        public bool Student { get; set; }

        public bool Contact { get; set; }

        public bool General { get; set; }

        // SEND - Special Education Needs and Disabilities
        public bool IsSend { get; set; }

        public bool IsSystem { get; set; }
    }
}