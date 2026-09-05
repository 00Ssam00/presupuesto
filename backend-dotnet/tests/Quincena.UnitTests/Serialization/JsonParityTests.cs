using System.Text.Json;
using Quincena.Api.Controllers;
using Quincena.Api.Serialization;
using Quincena.Domain.Entities;
using Xunit;

namespace Quincena.UnitTests.Serialization;

// "Fixture de paridad": el JSON producido por .NET debe ser idéntico byte a byte al que
// produce hoy el backend Node/Express + pg, porque el frontend React no cambia durante
// la migración del backend (ver plan de migración, sección "Paridad de serialización JSON").
// Los fixtures de este archivo fueron capturados con `curl` real contra el backend Node
// corriendo en localhost:3000.
public class JsonParityTests
{
    private static readonly JsonSerializerOptions Options = QuincenaJsonOptions.Create();

    [Fact]
    public void Health_coincide_con_el_shape_del_backend_Node()
    {
        // Fixture real: curl http://localhost:3000/api/health
        // {"status":"ok","timestamp":"2026-09-05T23:32:29.957Z"}
        var response = new HealthController.HealthResponse("ok", new DateTime(2026, 9, 5, 23, 32, 29, 957, DateTimeKind.Utc));

        var json = JsonSerializer.Serialize(response, Options);

        Assert.Equal("""{"status":"ok","timestamp":"2026-09-05T23:32:29.957Z"}""", json);
    }

    [Fact]
    public void Period_serializa_NUMERIC_como_string_y_DATE_sin_hora_igual_que_pg()
    {
        // Fixture real: curl http://localhost:3000/api/periods (primer elemento)
        // {"id":"3f1aced0-6890-4a92-adf3-da8135a0e0f6","label":"2da Quincena Diciembre",
        //  "start_date":"2026-12-16","end_date":"2026-12-31","base_income":"1113101.00",
        //  "rollover":"7188583.00","is_closed":false,"created_at":"2026-09-01T14:02:38.803Z"}
        var period = new Period
        {
            Id = Guid.Parse("3f1aced0-6890-4a92-adf3-da8135a0e0f6"),
            Label = "2da Quincena Diciembre",
            StartDate = new DateOnly(2026, 12, 16),
            EndDate = new DateOnly(2026, 12, 31),
            BaseIncome = 1113101.00m,
            Rollover = 7188583.00m,
            IsClosed = false,
            CreatedAt = new DateTime(2026, 9, 1, 14, 2, 38, 803, DateTimeKind.Utc),
        };

        var json = JsonSerializer.Serialize(period, Options);

        Assert.Equal(
            """{"id":"3f1aced0-6890-4a92-adf3-da8135a0e0f6","label":"2da Quincena Diciembre","start_date":"2026-12-16","end_date":"2026-12-31","base_income":"1113101.00","rollover":"7188583.00","is_closed":false,"created_at":"2026-09-01T14:02:38.803Z"}""",
            json);
    }

    [Fact]
    public void Percentage_nullable_se_serializa_como_null_no_se_omite()
    {
        var budget = new Budget
        {
            Id = Guid.NewGuid(),
            PeriodId = Guid.NewGuid(),
            CategoryId = Guid.NewGuid(),
            Amount = 50000m,
            Percentage = null,
            CreatedAt = DateTime.UtcNow,
        };

        var json = JsonSerializer.Serialize(budget, Options);

        Assert.Contains("\"percentage\":null", json);
    }

    [Fact]
    public void Numero_entrante_como_JSON_number_se_deserializa_correctamente()
    {
        // El frontend, al crear una transacción, manda `amount` como JSON number plano
        // (parseFloat del form), no como string — a diferencia de cómo lo devuelve la API.
        var json = """{"amount": 50000.5}""";
        var doc = JsonDocument.Parse(json);
        var converter = new PgNumericJsonConverter();
        var reader = new Utf8JsonReader(System.Text.Encoding.UTF8.GetBytes(doc.RootElement.GetProperty("amount").GetRawText()));
        reader.Read();

        var value = converter.Read(ref reader, typeof(decimal), Options);

        Assert.Equal(50000.5m, value);
    }
}
