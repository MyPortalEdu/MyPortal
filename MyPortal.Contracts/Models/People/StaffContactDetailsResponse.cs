namespace MyPortal.Contracts.Models.People;

/// <summary>
/// Read payload for the Contact Details area: a person's owned emails and phone numbers, plus
/// the active type options for each so the editor's dropdowns are self-contained in one fetch.
/// Addresses are shared entities and land in a later slice with their own search-before-add flow.
/// </summary>
public class StaffContactDetailsResponse
{
    public List<PersonEmailResponse> Emails { get; set; } = [];
    public List<PersonPhoneResponse> Phones { get; set; } = [];
    public List<LookupResponse> EmailTypes { get; set; } = [];
    public List<LookupResponse> PhoneTypes { get; set; } = [];
}
