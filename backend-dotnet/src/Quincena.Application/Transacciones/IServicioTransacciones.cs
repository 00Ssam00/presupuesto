using Quincena.Domain.Entidades;

namespace Quincena.Application.Transacciones;

public interface IServicioTransacciones
{
    Task<List<TransaccionListaItem>> ListarAsync(Guid? periodId, string? type, Guid? categoryId);
    Task<TransaccionListaItem?> ObtenerPorIdAsync(Guid id);
    Task<Transaccion> CrearAsync(CrearTransaccionPeticion request);
    Task<Transaccion?> ActualizarAsync(Guid id, ActualizarTransaccionPeticion request);
    Task EliminarAsync(Guid id);
    Task<ResultadoImportacion> ImportarAsync(List<FilaImportacion> rows);
}
