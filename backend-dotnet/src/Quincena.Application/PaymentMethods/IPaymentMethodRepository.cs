using Quincena.Domain.Entities;

namespace Quincena.Application.PaymentMethods;

public interface IPaymentMethodRepository
{
    Task<List<PaymentMethod>> GetActiveOrderedAsync();
}
