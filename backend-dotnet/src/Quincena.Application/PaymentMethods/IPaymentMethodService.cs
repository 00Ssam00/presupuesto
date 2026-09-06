using Quincena.Domain.Entities;

namespace Quincena.Application.PaymentMethods;

public interface IPaymentMethodService
{
    Task<List<PaymentMethod>> ListAsync();
}
