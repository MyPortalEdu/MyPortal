using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("SystemSettings")]
    public class SystemSetting
    {
        [Column(Order = 0)]
        [Key]
        public string Name { get; set; } = null!;

        [Column(Order = 1)]
        public string Setting { get; set; } = null!;
    }
}