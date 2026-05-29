using System.Text.Json.Serialization;

namespace MyPortal.Contracts.Models.People;

/// <summary>
/// High-level status badge for the staff header. <c>Leaver</c> (a staff member with an
/// employment whose end date has passed) lands with the employment-section slice.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum StaffStatus
{
    Active,
    Inactive
}
