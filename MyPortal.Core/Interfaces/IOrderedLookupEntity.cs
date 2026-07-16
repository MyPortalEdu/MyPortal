namespace MyPortal.Core.Interfaces
{
    /// <summary>
    /// A lookup whose options have a deliberate display order (e.g. ethnicity groupings, or
    /// "Other"/"Prefer not to say" pinned last) rather than plain alphabetical. Ordered by
    /// <see cref="DisplayOrder"/> then Description when surfaced to the UI.
    /// </summary>
    public interface IOrderedLookupEntity : IEntity
    {
        int DisplayOrder { get; set; }
    }
}
