using System.Globalization;
using System.Text.RegularExpressions;
using Quincena.Application.Tarjetas;
using Quincena.Application.Categorias;
using Quincena.Application.MetodosPago;
using Quincena.Application.Periodos;
using Quincena.Application.Arrastre;
using Quincena.Domain.Entidades;

namespace Quincena.Application.Transacciones;

// Port de backend/src/controllers/transactionsController.ts (incluye importTransactions).
public partial class ServicioTransacciones : IServicioTransacciones
{
    private readonly IRepositorioTransacciones _repository;
    private readonly IServicioArrastre _rolloverService;
    private readonly IRepositorioPeriodos _periodRepository;
    private readonly IRepositorioCategorias _categoryRepository;
    private readonly IRepositorioMetodosPago _paymentMethodRepository;
    private readonly IRepositorioTarjetas _cardRepository;

    public ServicioTransacciones(
        IRepositorioTransacciones repository,
        IServicioArrastre rolloverService,
        IRepositorioPeriodos periodRepository,
        IRepositorioCategorias categoryRepository,
        IRepositorioMetodosPago paymentMethodRepository,
        IRepositorioTarjetas cardRepository)
    {
        _repository = repository;
        _rolloverService = rolloverService;
        _periodRepository = periodRepository;
        _categoryRepository = categoryRepository;
        _paymentMethodRepository = paymentMethodRepository;
        _cardRepository = cardRepository;
    }

    public Task<List<TransaccionListaItem>> ListarAsync(Guid? periodId, string? type, Guid? categoryId) =>
        _repository.ListarAsync(periodId, type, categoryId);

    public Task<TransaccionListaItem?> ObtenerPorIdAsync(Guid id) => _repository.ObtenerPorIdConJoinsAsync(id);

    public async Task<Transaccion> CrearAsync(CrearTransaccionPeticion request)
    {
        var transaction = new Transaccion
        {
            PeriodoId = request.PeriodoId,
            CategoriaId = request.CategoriaId,
            MetodoPagoId = request.MetodoPagoId,
            TarjetaId = request.TarjetaId,
            Tipo = request.Tipo,
            Monto = request.Monto,
            Descripcion = request.Descripcion,
            Nota = request.Nota,
            Fecha = request.Fecha,
        };
        var created = await _repository.AgregarAsync(transaction);
        await _rolloverService.RecalcularTodoAsync();
        return created;
    }

    public async Task<Transaccion?> ActualizarAsync(Guid id, ActualizarTransaccionPeticion request)
    {
        var updated = await _repository.ActualizarAsync(id, t =>
        {
            t.CategoriaId = request.CategoriaId;
            t.MetodoPagoId = request.MetodoPagoId;
            t.TarjetaId = request.TarjetaId;
            t.Tipo = request.Tipo;
            t.Monto = request.Monto;
            t.Descripcion = request.Descripcion;
            t.Nota = request.Nota;
            t.Fecha = request.Fecha;
            t.ActualizadoEn = DateTime.UtcNow;
        });
        if (updated == null) return null;

        await _rolloverService.RecalcularTodoAsync();
        return updated;
    }

    public async Task EliminarAsync(Guid id)
    {
        // Igual que Node: no valida existencia previa, siempre recalcula.
        await _repository.EliminarAsync(id);
        await _rolloverService.RecalcularTodoAsync();
    }

    [GeneratedRegex(@"^\d{4}-\d{2}-\d{2}$")]
    private static partial Regex PatronFormatoFecha();

    private static T? BuscarPorNombre<T>(List<T> list, Func<T, string> nameSelector, string? name) where T : class
    {
        var needle = (name ?? "").Trim().ToLowerInvariant();
        return list.FirstOrDefault(x => nameSelector(x).ToLowerInvariant() == needle);
    }

    // La quincena de cada fila se resuelve por su fecha (obligatoria en el CSV) contra
    // el rango de las quincenas existentes, no por una única quincena elegida por el
    // usuario — así un archivo que abarca varias quincenas se importa en una sola
    // pasada. Importación parcial: cada fila se valida y reporta por separado.
    //
    // A diferencia de Node (que inserta todas las filas válidas en paralelo con
    // Promise.all), acá se insertan secuencialmente porque un solo DbContext de EF
    // Core no es seguro para operaciones concurrentes — mismo criterio ya aplicado
    // en FuenteDatosArrastre. El orden final de `results` por fila es idéntico.
    public async Task<ResultadoImportacion> ImportarAsync(List<FilaImportacion> rows)
    {
        var periods = await _periodRepository.ObtenerTodosOrdenadosPorFechaInicioDescAsync();
        var categories = await _categoryRepository.ObtenerActivosOrdenadosAsync();
        var methods = await _paymentMethodRepository.ObtenerActivosOrdenadosAsync();
        var cards = await _cardRepository.ObtenerActivosOrdenadosAsync();

        var results = new ResultadoFilaImportacion?[rows.Count];
        var toInsert = new List<(int RowIndex, Transaccion Transaccion)>();

        for (var i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            var rowNum = i + 1;

            var montoRaw = row.Monto ?? "";
            if (string.IsNullOrEmpty(row.Monto) ||
                !decimal.TryParse(montoRaw.Trim(), NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var amount) ||
                amount <= 0)
            {
                results[i] = new ResultadoFilaImportacion { Fila = rowNum, Estado = "error", Error = $"Monto inválido: \"{montoRaw}\"" };
                continue;
            }

            var type = (row.Tipo ?? "").Trim().ToLowerInvariant();
            if (type != "income" && type != "expense")
            {
                results[i] = new ResultadoFilaImportacion { Fila = rowNum, Estado = "error", Error = $"Tipo debe ser \"income\" o \"expense\", no \"{row.Tipo ?? ""}\"" };
                continue;
            }

            var dateStr = (row.Fecha ?? "").Trim();
            // Node solo valida el formato con regex (no la validez calendárica real);
            // acá además exigimos que parseé a una fecha real — divergencia menor y
            // deliberada, sin caso de prueba que dependa de fechas con formato válido
            // pero calendario inválido (ej. mes 13).
            if (!PatronFormatoFecha().IsMatch(dateStr) || !DateOnly.TryParseExact(dateStr, "yyyy-MM-dd", out var date))
            {
                results[i] = new ResultadoFilaImportacion { Fila = rowNum, Estado = "error", Error = $"Fecha requerida e inválida: \"{dateStr}\" (usar YYYY-MM-DD)" };
                continue;
            }

            var period = periods.FirstOrDefault(p => date >= p.FechaInicio && date <= p.FechaFin);
            if (period == null)
            {
                results[i] = new ResultadoFilaImportacion { Fila = rowNum, Estado = "error", Error = $"No hay ninguna quincena que incluya la fecha {dateStr}" };
                continue;
            }

            var category = BuscarPorNombre(categories, c => c.Nombre, row.Categoria);
            if (category == null)
            {
                results[i] = new ResultadoFilaImportacion { Fila = rowNum, Estado = "error", Error = $"Categoría no encontrada: \"{row.Categoria ?? ""}\"" };
                continue;
            }

            var method = BuscarPorNombre(methods, m => m.Nombre, row.Metodo);
            if (method == null)
            {
                results[i] = new ResultadoFilaImportacion { Fila = rowNum, Estado = "error", Error = $"Método de pago no encontrado: \"{row.Metodo ?? ""}\"" };
                continue;
            }

            Guid? cardId = null;
            if (method.RequiereTarjeta)
            {
                var card = BuscarPorNombre(cards, c => c.Nombre, row.Tarjeta);
                if (card == null)
                {
                    results[i] = new ResultadoFilaImportacion { Fila = rowNum, Estado = "error", Error = $"El método \"{method.Nombre}\" requiere tarjeta: \"{row.Tarjeta ?? ""}\" no encontrada" };
                    continue;
                }
                cardId = card.Id;
            }

            var description = (row.Descripcion ?? "").Trim();
            if (description.Length == 0) description = "Importado";

            toInsert.Add((i, new Transaccion
            {
                PeriodoId = period.Id,
                CategoriaId = category.Id,
                MetodoPagoId = method.Id,
                TarjetaId = cardId,
                Tipo = type,
                Monto = amount,
                Descripcion = description,
                Fecha = date,
            }));
        }

        foreach (var (rowIndex, transaction) in toInsert)
        {
            var created = await _repository.AgregarAsync(transaction);
            results[rowIndex] = new ResultadoFilaImportacion { Fila = rowIndex + 1, Estado = "created", Id = created.Id };
        }

        if (toInsert.Count > 0) await _rolloverService.RecalcularTodoAsync();

        return new ResultadoImportacion
        {
            Creados = toInsert.Count,
            Fallidos = rows.Count - toInsert.Count,
            Resultados = results.Select(r => r!).ToList(),
        };
    }
}
