using System.ComponentModel.DataAnnotations;

namespace MyPortal.Contracts.Models.People;

public class TrainingCourseResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public bool Active { get; set; }

    public bool InUse { get; set; }
}

public class TrainingCourseUpsertRequest
{
    [Required]
    [StringLength(128)]
    public string Code { get; set; } = null!;

    [Required]
    [StringLength(128)]
    public string Name { get; set; } = null!;

    [StringLength(256)]
    public string? Description { get; set; }

    public bool Active { get; set; } = true;
}
