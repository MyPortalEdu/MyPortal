using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("Comments")]
    public class Comment : Entity
    {
        public Guid CommentTypeId { get; set; }

        public Guid CommentBankSectionId { get; set; }

        [Required]
        public string Value { get; set; } = null!;

        public CommentType? CommentType { get; set; }
        public CommentBankSection? Section { get; set; }
    }
}