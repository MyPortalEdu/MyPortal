namespace MyPortal.Contracts.Models.People;

/// <summary>
/// What the current viewer may do with one section of a specific staff member, resolved for the
/// (viewer, subject) pair. The front end renders the profile sidebar from a map of these and
/// never hardcodes visibility.
/// </summary>
public sealed class SectionAccess
{
    public bool CanView { get; init; }
    public bool CanEdit { get; init; }

    public static readonly SectionAccess None = new() { CanView = false, CanEdit = false };
}
