using Microsoft.EntityFrameworkCore;
using Quincena.Application.Transactions;
using Quincena.Domain.Entities;

namespace Quincena.Infrastructure.Repositories;

// Los entities Transaction/Category/PaymentMethod/Card no tienen propiedades de
// navegación (ver sus Configuration), así que los JOIN de SELECT_WITH_JOINS de
// Node se replican acá como joins LINQ manuales sobre las FKs, no con Include().
public class TransactionRepository : ITransactionRepository
{
    private readonly QuincenaDbContext _db;

    public TransactionRepository(QuincenaDbContext db)
    {
        _db = db;
    }

    private IQueryable<TransactionListItem> JoinedQuery() =>
        from t in _db.Transactions
        join c in _db.Categories on t.CategoryId equals c.Id
        join pm in _db.PaymentMethods on t.PaymentMethodId equals pm.Id
        join ca in _db.Cards on t.CardId equals ca.Id into cardJoin
        from ca in cardJoin.DefaultIfEmpty()
        select new TransactionListItem
        {
            Id = t.Id,
            PeriodId = t.PeriodId,
            CategoryId = t.CategoryId,
            PaymentMethodId = t.PaymentMethodId,
            CardId = t.CardId,
            Type = t.Type,
            Amount = t.Amount,
            Description = t.Description,
            Note = t.Note,
            Date = t.Date,
            CreatedAt = t.CreatedAt,
            UpdatedAt = t.UpdatedAt,
            CategoryName = c.Name,
            CategoryColor = c.Color,
            PaymentMethodName = pm.Name,
            CardName = ca != null ? ca.Name : null,
        };

    public async Task<List<TransactionListItem>> ListAsync(Guid? periodId, string? type, Guid? categoryId)
    {
        var query = JoinedQuery();
        if (periodId.HasValue) query = query.Where(x => x.PeriodId == periodId.Value);
        if (!string.IsNullOrEmpty(type)) query = query.Where(x => x.Type == type);
        if (categoryId.HasValue) query = query.Where(x => x.CategoryId == categoryId.Value);

        return await query
            .OrderByDescending(x => x.Date).ThenByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public Task<TransactionListItem?> GetByIdWithJoinsAsync(Guid id) =>
        JoinedQuery().FirstOrDefaultAsync(x => x.Id == id);

    public async Task<Transaction> AddAsync(Transaction transaction)
    {
        _db.Transactions.Add(transaction);
        await _db.SaveChangesAsync();
        return transaction;
    }

    public async Task<Transaction?> UpdateAsync(Guid id, Action<Transaction> apply)
    {
        var transaction = await _db.Transactions.FirstOrDefaultAsync(t => t.Id == id);
        if (transaction == null) return null;
        apply(transaction);
        await _db.SaveChangesAsync();
        return transaction;
    }

    public async Task DeleteAsync(Guid id)
    {
        var transaction = await _db.Transactions.FirstOrDefaultAsync(t => t.Id == id);
        if (transaction == null) return;
        _db.Transactions.Remove(transaction);
        await _db.SaveChangesAsync();
    }
}
