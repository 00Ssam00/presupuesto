using System.Text.Json;
using Quincena.Api.Controladores;
using Quincena.Api.Serialization;
using Quincena.Domain.Entidades;
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
        // Fixture real capturado originalmente contra Node (curl http://localhost:3000/api/health),
        // con las claves ya traducidas tras el renombrado a español ("status" -> "estado").
        var response = new ControladorSalud.RespuestaSalud("ok", new DateTime(2026, 9, 5, 23, 32, 29, 957, DateTimeKind.Utc));

        var json = JsonSerializer.Serialize(response, Options);

        Assert.Equal("""{"estado":"ok","timestamp":"2026-09-05T23:32:29.957Z"}""", json);
    }

    [Fact]
    public void Period_serializa_NUMERIC_como_string_y_DATE_sin_hora_igual_que_pg()
    {
        // Fixture real capturado originalmente contra Node (curl http://localhost:3000/api/periods,
        // primer elemento), con las claves ya traducidas tras el renombrado a español:
        // {"id":"3f1aced0-6890-4a92-adf3-da8135a0e0f6","etiqueta":"2da Quincena Diciembre",
        //  "fecha_inicio":"2026-12-16","fecha_fin":"2026-12-31","ingreso_base":"1113101.00",
        //  "arrastre":"7188583.00","esta_cerrado":false,"creado_en":"2026-09-01T14:02:38.803Z"}
        var period = new Periodo
        {
            Id = Guid.Parse("3f1aced0-6890-4a92-adf3-da8135a0e0f6"),
            Etiqueta = "2da Quincena Diciembre",
            FechaInicio = new DateOnly(2026, 12, 16),
            FechaFin = new DateOnly(2026, 12, 31),
            IngresoBase = 1113101.00m,
            Arrastre = 7188583.00m,
            EstaCerrado = false,
            CreadoEn = new DateTime(2026, 9, 1, 14, 2, 38, 803, DateTimeKind.Utc),
        };

        var json = JsonSerializer.Serialize(period, Options);

        Assert.Equal(
            """{"id":"3f1aced0-6890-4a92-adf3-da8135a0e0f6","etiqueta":"2da Quincena Diciembre","fecha_inicio":"2026-12-16","fecha_fin":"2026-12-31","ingreso_base":"1113101.00","arrastre":"7188583.00","esta_cerrado":false,"creado_en":"2026-09-01T14:02:38.803Z"}""",
            json);
    }

    [Fact]
    public void Percentage_nullable_se_serializa_como_null_no_se_omite()
    {
        var budget = new Presupuesto
        {
            Id = Guid.NewGuid(),
            PeriodoId = Guid.NewGuid(),
            CategoriaId = Guid.NewGuid(),
            Monto = 50000m,
            Porcentaje = null,
            CreadoEn = DateTime.UtcNow,
        };

        var json = JsonSerializer.Serialize(budget, Options);

        Assert.Contains("\"porcentaje\":null", json);
    }

    [Fact]
    public void Numero_entrante_como_JSON_number_se_deserializa_correctamente()
    {
        // El frontend, al crear una transacción, manda `monto` como JSON number plano
        // (parseFloat del form), no como string — a diferencia de cómo lo devuelve la API.
        var json = """{"monto": 50000.5}""";
        var doc = JsonDocument.Parse(json);
        var converter = new PgNumericJsonConverter();
        var reader = new Utf8JsonReader(System.Text.Encoding.UTF8.GetBytes(doc.RootElement.GetProperty("monto").GetRawText()));
        reader.Read();

        var value = converter.Read(ref reader, typeof(decimal), Options);

        Assert.Equal(50000.5m, value);
    }
}
