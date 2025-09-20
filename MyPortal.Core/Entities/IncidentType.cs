using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("IncidentTypes")]
    public class IncidentType : LookupEntity
    {
        public int DefaultPoints { get; set; }
    }
}