using Quincena.Domain.Entities;

namespace Quincena.Application.Transactions;

public interface ITransactionService
{
    Task<List<TransactionListItem>> ListAsync(Guid? periodId, string? type, Guid? categoryId);
    Task<TransactionListItem?> GetByIdAsync(Guid id);
    Task<Transaction> CreateAsync(CreateTransactionRequest request);
    Task<Transaction?> UpdateAsync(Guid id, UpdateTransactionRequest request);
    Task DeleteAsync(Guid id);
    Task<ImportResult> ImportAsync(List<ImportRow> rows);
}
