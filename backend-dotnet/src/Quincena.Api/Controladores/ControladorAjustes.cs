using Microsoft.AspNetCore.Mvc;
using Quincena.Application.Ajustes;

namespace Quincena.Api.Controladores;

// Port de backend/src/controllers/settingsController.ts.
[ApiController]
[Route("api/ajustes")]
public class ControladorAjustes : ControllerBase
{
    private readonly IServicioAjustes _service;

    public ControladorAjustes(IServicioAjustes service)
    {
        _service = service;
    }

    public class ActualizarAjustePeticion
    {
        public string Valor { get; set; } = null!;
    }

    [HttpGet]
    public async Task<IActionResult> Get() => Ok(await _service.ObtenerTodosAsync());

    [HttpPut("{clave}")]
    public async Task<IActionResult> Update(string clave, [FromBody] ActualizarAjustePeticion request)
    {
        await _service.ActualizarAsync(clave, request.Valor);
        return Ok(new { clave, valor = request.Valor });
    }
}
