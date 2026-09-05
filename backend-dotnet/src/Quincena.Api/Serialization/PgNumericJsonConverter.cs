using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Quincena.Api.Serialization;

// El driver `pg` de Node devuelve las columnas NUMERIC como string (para no perder precisión),
// y el frontend React espera ese mismo shape ("amount": "1000000.00", no 1000000.00 sin comillas).
// Todas las columnas NUMERIC del esquema usan escala 2, así que "F2" es válido en todos los casos.
// En la entrada (POST/PUT), el frontend sí manda un JSON number plano (ej. parseFloat del form),
// así que la lectura acepta ambos: number o string.
public class PgNumericJsonConverter : JsonConverter<decimal>
{
    public override decimal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var s = reader.GetString();
            return decimal.Parse(s!, NumberStyles.Number, CultureInfo.InvariantCulture);
        }
        return reader.GetDecimal();
    }

    public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString("F2", CultureInfo.InvariantCulture));
    }
}
