using Microsoft.AspNetCore.Mvc;
using Quincena.Application.PaymentMethods;

namespace Quincena.Api.Controllers;

// Port de backend/src/controllers/paymentMethodsController.ts (solo lectura).
[ApiController]
[Route("api/payment-methods")]
public class PaymentMethodsController : ControllerBase
{
    private readonly IPaymentMethodService _service;

    public PaymentMethodsController(IPaymentMethodService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> List() => Ok(await _service.ListAsync());
}
