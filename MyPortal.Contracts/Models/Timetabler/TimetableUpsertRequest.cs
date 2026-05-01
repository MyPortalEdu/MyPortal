using System.ComponentModel.DataAnnotations;

namespace MyPortal.Contracts.Models.Timetabler;

public class TimetableUpsertRequest
{
    [Required]
    public Guid AcademicYearId { get; set; }

    [Required, StringLength(128)]
    public string Name { get; set; } = null!;
}
