namespace MyPortal.Contracts.Models;

/// <summary>
/// Tiny envelope returned by create / upsert endpoints that produce a new entity id.
/// Wrapping the id in a typed response (rather than an anonymous <c>{ id }</c>
/// shape) makes the OpenAPI schema concrete so generated clients and the Scalar
/// preview render meaningful types.
/// </summary>
public sealed class IdResponse
{
    /// <summary>The id of the entity that was created or updated.</summary>
    public Guid Id { get; init; }
}
