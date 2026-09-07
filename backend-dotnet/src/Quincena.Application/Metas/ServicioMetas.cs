using Quincena.Domain.Entidades;

namespace Quincena.Application.Metas;

// Port de backend/src/controllers/goalsController.ts.
public class ServicioMetas : IServicioMetas
{
    private readonly IRepositorioMetas _repository;

    public ServicioMetas(IRepositorioMetas repository)
    {
        _repository = repository;
    }

    public Task<List<Meta>> ListarAsync() => _repository.ObtenerActivasOrdenadasAsync();

    public Task<Meta> CrearAsync(CrearMetaPeticion request)
    {
        var goal = new Meta
        {
            Nombre = request.Nombre,
            Tipo = request.Tipo,
            MontoObjetivo = request.MontoObjetivo,
            MontoActual = request.MontoActual ?? 0m,
            FechaLimite = request.FechaLimite,
        };
        return _repository.AgregarAsync(goal);
    }

    public Task<Meta?> ActualizarAsync(Guid id, ActualizarMetaPeticion request) =>
        _repository.ActualizarAsync(id, goal =>
        {
            goal.Nombre = request.Nombre;
            goal.Tipo = request.Tipo;
            goal.MontoObjetivo = request.MontoObjetivo;
            goal.MontoActual = request.MontoActual;
            goal.FechaLimite = request.FechaLimite;
            // COALESCE: is_active solo se pisa si vino en el body (regresión
            // en Node: el form de edición no lo mandaba y la meta desaparecía).
            if (request.EstaActivo.HasValue) goal.EstaActivo = request.EstaActivo.Value;
        });

    public Task EliminarAsync(Guid id) => _repository.DesactivarAsync(id);
}
