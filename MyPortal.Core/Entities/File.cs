using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("Files")]
    public class File : Entity
    {
        [Required] 
        public required string FileId { get; set; }

        [Required] 
        public required string FileName { get; set; }

        [Required] 
        public required string ContentType { get; set; }
    }
}