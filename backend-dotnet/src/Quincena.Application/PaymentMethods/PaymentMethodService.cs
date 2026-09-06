using Quincena.Domain.Entities;

namespace Quincena.Application.PaymentMethods;

// Port de backend/src/controllers/paymentMethodsController.ts (solo lectura,
// no hay create/update/delete en Node para este recurso).
public class PaymentMethodService : IPaymentMethodService
{
    private readonly IPaymentMethodRepository _repository;

    public PaymentMethodService(IPaymentMethodRepository repository)
    {
        _repository = repository;
    }

    public Task<List<PaymentMethod>> ListAsync() => _repository.GetActiveOrderedAsync();
}
