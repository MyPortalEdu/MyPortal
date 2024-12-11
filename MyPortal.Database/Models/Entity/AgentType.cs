using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Database.BaseTypes;

namespace MyPortal.Database.Models.Entity
{
    [Table("agent_type")]
    public class AgentType : LookupItem
    {
        public virtual ICollection<Agent> Agents { get; set; }
    }
}