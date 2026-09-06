using Microsoft.EntityFrameworkCore;
using Quincena.Application.PaymentMethods;
using Quincena.Domain.Entities;

namespace Quincena.Infrastructure.Repositories;

public class PaymentMethodRepository : IPaymentMethodRepository
{
    private readonly QuincenaDbContext _db;

    public PaymentMethodRepository(QuincenaDbContext db)
    {
        _db = db;
    }

    public Task<List<PaymentMethod>> GetActiveOrderedAsync() =>
        _db.PaymentMethods.Where(p => p.IsActive).OrderBy(p => p.Name).ToListAsync();
}
