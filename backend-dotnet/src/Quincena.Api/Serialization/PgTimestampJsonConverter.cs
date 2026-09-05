using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Quincena.Api.Serialization;

// Node serializa TIMESTAMPTZ vía `Date.toJSON()`, que produce milisegundos con 3 dígitos
// ("2026-09-01T12:34:56.789Z"). El formato "O" por defecto de System.Text.Json usa 7 dígitos
// de fracción, lo que rompería la paridad byte a byte con el frontend.
public class PgTimestampJsonConverter : JsonConverter<DateTime>
{
    private const string Format = "yyyy-MM-ddTHH:mm:ss.fffZ";

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var s = reader.GetString();
        return DateTime.Parse(s!, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal);
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToUniversalTime().ToString(Format, CultureInfo.InvariantCulture));
    }
}
