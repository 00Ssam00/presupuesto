using Quincena.Domain.Entidades;

namespace Quincena.Application.Transacciones;

public interface IRepositorioTransacciones
{
    Task<List<TransaccionListaItem>> ListarAsync(Guid? periodId, string? type, Guid? categoryId);
    Task<TransaccionListaItem?> ObtenerPorIdConJoinsAsync(Guid id);

    // Sin joins: igual que el `RETURNING *` de Node en INSERT/UPDATE, que no
    // incluye category_name/payment_method_name/card_name.
    Task<Transaccion> AgregarAsync(Transaccion transaction);
    Task<Transaccion?> ActualizarAsync(Guid id, Action<Transaccion> apply);

    // No valida existencia previa, igual que Node.
    Task EliminarAsync(Guid id);
}
