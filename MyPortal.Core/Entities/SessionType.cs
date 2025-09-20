using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities;

[Table("SessionTypes")]
public class SessionType : LookupEntity
{
    public string? Code { get; set; }

    public int Length { get; set; }
}