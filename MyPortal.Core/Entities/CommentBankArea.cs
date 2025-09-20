using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities;

[Table("CommentBankAreas")]
public class CommentBankArea : Entity
{
    public Guid CommentBankId { get; set; }

    public Guid CourseId { get; set; }

    [Required, StringLength(256)]
    public required string Name { get; set; }

    public CommentBank? CommentBank { get; set; }
    public Course? Course { get; set; }
}