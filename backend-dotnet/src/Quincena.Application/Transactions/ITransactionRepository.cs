using Quincena.Domain.Entities;

namespace Quincena.Application.Transactions;

public interface ITransactionRepository
{
    Task<List<TransactionListItem>> ListAsync(Guid? periodId, string? type, Guid? categoryId);
    Task<TransactionListItem?> GetByIdWithJoinsAsync(Guid id);

    // Sin joins: igual que el `RETURNING *` de Node en INSERT/UPDATE, que no
    // incluye category_name/payment_method_name/card_name.
    Task<Transaction> AddAsync(Transaction transaction);
    Task<Transaction?> UpdateAsync(Guid id, Action<Transaction> apply);

    // No valida existencia previa, igual que Node.
    Task DeleteAsync(Guid id);
}
