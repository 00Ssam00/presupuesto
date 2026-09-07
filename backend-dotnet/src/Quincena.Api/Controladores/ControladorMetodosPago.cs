using Microsoft.AspNetCore.Mvc;
using Quincena.Application.MetodosPago;

namespace Quincena.Api.Controladores;

// Port de backend/src/controllers/paymentMethodsController.ts (solo lectura).
[ApiController]
[Route("api/metodos-pago")]
public class ControladorMetodosPago : ControllerBase
{
    private readonly IServicioMetodosPago _service;

    public ControladorMetodosPago(IServicioMetodosPago service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> List() => Ok(await _service.ListarAsync());
}
