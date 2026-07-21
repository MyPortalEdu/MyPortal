namespace MyPortal.Contracts.Models.People;

/// <summary>
/// A single phone number owned by a person. <see cref="TypeId"/> resolves against the
/// PhoneTypes list on <see cref="PersonContactDetailsResponse"/>.
/// </summary>
public class PersonPhoneResponse
{
    public Guid Id { get; set; }
    public Guid TypeId { get; set; }
    public string Number { get; set; } = null!;
    public bool IsMain { get; set; }
}
