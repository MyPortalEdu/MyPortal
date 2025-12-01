using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities;

[Table("CommentBankSections")]
public class CommentBankSection : Entity
{
    public Guid CommentBankAreaId { get; set; }

    [Required, StringLength(256)] 
    public string Name { get; set; } = null!;

    public CommentBankArea? Area { get; set; }
}