using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("LocalAuthorities")]
    public class LocalAuthority : Entity
    {
        public int LeaCode { get; set; }

        [Required]
        [StringLength(128)] 
        public string Name { get; set; } = null!;

        [Url, StringLength(100)]
        public string? Website { get; set; }
    }
}