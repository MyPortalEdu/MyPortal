namespace MyPortal.Contracts.Models;

/// <summary>
/// Result of an advisory "is this value available?" check (e.g. a staff code or school URN not
/// already in use). Wrapping the boolean in a typed response (rather than an anonymous
/// <c>{ available }</c> shape) keeps the OpenAPI schema concrete for generated clients. These
/// checks exist for inline UI feedback; the authoritative uniqueness guard still runs on save.
/// </summary>
public sealed class AvailabilityResponse
{
    /// <summary>True when the value is not already in use (blank values are treated as available).</summary>
    public bool Available { get; init; }
}
