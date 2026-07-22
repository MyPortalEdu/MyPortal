using System.Text.Json.Serialization;

namespace MyPortal.Contracts.Models.People;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AddressEditMode
{
    FixInPlace,
    Moved
}
