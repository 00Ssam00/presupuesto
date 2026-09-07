using Quincena.Application.Arrastre;
using Quincena.Domain.Entidades;

namespace Quincena.Application.Periodos;

// Port de backend/src/controllers/periodsController.ts. Tras cada mutación que afecta
// el rollover (crear/editar/eliminar), se recalcula y se relee el registro fresco,
// igual que el original.
public class ServicioPeriodos : IServicioPeriodos
{
    private readonly IRepositorioPeriodos _repository;
    private readonly IServicioArrastre _rolloverService;

    public ServicioPeriodos(IRepositorioPeriodos repository, IServicioArrastre rolloverService)
    {
        _repository = repository;
        _rolloverService = rolloverService;
    }

    public Task<List<Periodo>> ListarAsync() => _repository.ObtenerTodosOrdenadosPorFechaInicioDescAsync();

    public Task<Periodo?> ObtenerActualAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return _repository.ObtenerVigenteOMasRecienteAsync(today);
    }

    public Task<Periodo?> ObtenerPorIdAsync(Guid id) => _repository.ObtenerPorIdAsync(id);

    public async Task<Periodo> CrearAsync(CrearPeriodoPeticion request)
    {
        var period = new Periodo
        {
            Etiqueta = request.Etiqueta,
            FechaInicio = request.FechaInicio,
            FechaFin = request.FechaFin,
            IngresoBase = request.IngresoBase ?? 0m,
            Arrastre = request.Arrastre ?? 0m,
        };

        var created = await _repository.AgregarAsync(period);
        await _rolloverService.RecalcularTodoAsync();
        return (await _repository.ObtenerPorIdAsync(created.Id))!;
    }

    public async Task<Periodo?> ActualizarAsync(Guid id, ActualizarPeriodoPeticion request)
    {
        var found = await _repository.ActualizarAsync(id, period =>
        {
            period.Etiqueta = request.Etiqueta;
            period.FechaInicio = request.FechaInicio;
            period.FechaFin = request.FechaFin;
            period.IngresoBase = request.IngresoBase;
            period.Arrastre = request.Arrastre;
            period.EstaCerrado = request.EstaCerrado;
        });

        if (!found) return null;

        await _rolloverService.RecalcularTodoAsync();
        return await _repository.ObtenerPorIdAsync(id);
    }

    public async Task<bool> EliminarAsync(Guid id)
    {
        // Igual que Node: no valida existencia previa, siempre recalcula.
        await _repository.EliminarAsync(id);
        await _rolloverService.RecalcularTodoAsync();
        return true;
    }
}
