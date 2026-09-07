using Quincena.Domain.Entidades;

namespace Quincena.Application.Tarjetas;

// Port de backend/src/controllers/cardsController.ts.
public class ServicioTarjetas : IServicioTarjetas
{
    private readonly IRepositorioTarjetas _repository;

    public ServicioTarjetas(IRepositorioTarjetas repository)
    {
        _repository = repository;
    }

    public Task<List<Tarjeta>> ListarAsync() => _repository.ObtenerActivosOrdenadosAsync();

    public Task<Tarjeta> CrearAsync(CrearTarjetaPeticion request)
    {
        var card = new Tarjeta
        {
            Nombre = request.Nombre,
            UltimosCuatro = string.IsNullOrEmpty(request.UltimosCuatro) ? null : request.UltimosCuatro,
            Banco = string.IsNullOrEmpty(request.Banco) ? null : request.Banco,
        };
        return _repository.AgregarAsync(card);
    }

    public Task<Tarjeta?> ActualizarAsync(Guid id, ActualizarTarjetaPeticion request) =>
        _repository.ActualizarAsync(id, card =>
        {
            card.Nombre = request.Nombre;
            card.UltimosCuatro = string.IsNullOrEmpty(request.UltimosCuatro) ? null : request.UltimosCuatro;
            card.Banco = string.IsNullOrEmpty(request.Banco) ? null : request.Banco;
            // COALESCE: is_active solo se pisa si vino en el body.
            if (request.EstaActivo.HasValue) card.EstaActivo = request.EstaActivo.Value;
        });

    public Task EliminarAsync(Guid id) => _repository.DesactivarAsync(id);
}
