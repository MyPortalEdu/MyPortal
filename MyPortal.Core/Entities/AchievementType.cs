using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("AchievementTypes")]
    public class AchievementType : LookupEntity
    {
        public int DefaultPoints { get; set; }
    }
}