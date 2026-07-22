using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    // A designated staff responsibility / duty (Designated Safeguarding Lead, First Aider, SENCO,
    // Fire Marshal, …). Distinct from the census job role on the contract — a person holds these on
    // top of their post, each for a period.
    [Table("StaffResponsibilityTypes")]
    public class StaffResponsibilityType : LookupEntity, ISystemEntity, IOrderedLookupEntity
    {
        public bool IsSystem { get; set; }

        public int DisplayOrder { get; set; }
    }
}
