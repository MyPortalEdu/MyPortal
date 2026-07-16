using System.Text.Json.Serialization;

namespace MyPortal.Contracts.Models.People;

/// <summary>
/// How to apply an edit to a <em>shared</em> address. Irrelevant when the editor is the only
/// person linked — the server edits in place regardless.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AddressEditMode
{
    /// <summary>Edit the shared address in place — the change applies to everyone linked.</summary>
    FixInPlace,

    /// <summary>They moved — fork a new address with the edits and relink just this person.</summary>
    Moved
}
