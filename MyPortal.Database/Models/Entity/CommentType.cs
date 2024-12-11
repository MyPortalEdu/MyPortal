using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Database.Models.Entity;

[Table("comment_type")]
public class CommentType : BaseTypes.LookupItem
{
    public virtual ICollection<Comment> Comments { get; set; }
}