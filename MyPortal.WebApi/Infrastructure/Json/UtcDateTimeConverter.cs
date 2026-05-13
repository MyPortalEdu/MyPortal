using System.Text.Json;
using System.Text.Json.Serialization;

namespace MyPortal.WebApi.Infrastructure.Json;

/// <summary>
/// Serializes every <see cref="DateTime"/> as a Z-suffixed UTC ISO string and
/// parses every inbound ISO string as UTC. The DB stores UTC instants in
/// <c>datetime2</c> columns; Dapper hands those back as <see cref="DateTime"/>
/// with <see cref="DateTimeKind.Unspecified"/>, which System.Text.Json would
/// otherwise emit without a TZ marker — and JavaScript's <c>new Date(s)</c>
/// then reads that as a local-time instant, shifting the value by the
/// browser's offset. Forcing UTC at the wire keeps both sides aligned.
/// </summary>
public sealed class UtcDateTimeConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetDateTime();
        return value.Kind == DateTimeKind.Utc ? value : DateTime.SpecifyKind(value, DateTimeKind.Utc);
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        var utc = value.Kind switch
        {
            DateTimeKind.Utc         => value,
            DateTimeKind.Local       => value.ToUniversalTime(),
            DateTimeKind.Unspecified => DateTime.SpecifyKind(value, DateTimeKind.Utc),
            _                        => value,
        };
        writer.WriteStringValue(utc.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ"));
    }
}

/// <summary>Nullable companion to <see cref="UtcDateTimeConverter"/>.</summary>
public sealed class UtcNullableDateTimeConverter : JsonConverter<DateTime?>
{
    public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null) return null;
        var value = reader.GetDateTime();
        return value.Kind == DateTimeKind.Utc ? value : DateTime.SpecifyKind(value, DateTimeKind.Utc);
    }

    public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
    {
        if (value is null) { writer.WriteNullValue(); return; }
        var utc = value.Value.Kind switch
        {
            DateTimeKind.Utc         => value.Value,
            DateTimeKind.Local       => value.Value.ToUniversalTime(),
            DateTimeKind.Unspecified => DateTime.SpecifyKind(value.Value, DateTimeKind.Utc),
            _                        => value.Value,
        };
        writer.WriteStringValue(utc.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ"));
    }
}
