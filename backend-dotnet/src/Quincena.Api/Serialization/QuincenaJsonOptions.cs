using System.Text.Json;

namespace Quincena.Api.Serialization;

// Configuración de serialización compartida entre Program.cs (vía AddJsonOptions) y los tests
// de paridad — un único lugar para no correr riesgo de que diverjan silenciosamente.
public static class QuincenaJsonOptions
{
    public static void Configure(JsonSerializerOptions options)
    {
        options.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
        options.Converters.Add(new PgNumericJsonConverter());
        options.Converters.Add(new PgTimestampJsonConverter());
    }

    public static JsonSerializerOptions Create()
    {
        var options = new JsonSerializerOptions();
        Configure(options);
        return options;
    }
}
