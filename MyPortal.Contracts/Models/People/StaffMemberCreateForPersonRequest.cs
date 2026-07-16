namespace MyPortal.Contracts.Models.People;

/// <summary>
/// Write payload for attaching a staff role to an existing <c>Person</c>. The person's bio is
/// already on file (it was created with another subtype role), so only the staff-specific
/// <see cref="Code"/> is supplied here — no bio fields, which would risk silently overwriting
/// the shared Person record. Used by the "new staff member" flow when HR picks an existing
/// person rather than creating a fresh one.
/// </summary>
public class StaffMemberCreateForPersonRequest
{
    public Guid PersonId { get; set; }
    public string Code { get; set; } = null!;
}
