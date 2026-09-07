using Microsoft.AspNetCore.Mvc;
using Quincena.Application.Panel;

namespace Quincena.Api.Controladores;

// Port de backend/src/controllers/dashboardController.ts.
[ApiController]
[Route("api/panel")]
public class ControladorPanel : ControllerBase
{
    private readonly IServicioPanel _service;

    public ControladorPanel(IServicioPanel service)
    {
        _service = service;
    }

    [HttpGet("{periodId:guid}")]
    public async Task<IActionResult> Get(Guid periodId)
    {
        var result = await _service.ObtenerPanelAsync(periodId);
        // Mensaje de error propio de este endpoint ("Período no encontrado"),
        // distinto del "No encontrado" genérico del resto de la app — así lo tiene Node.
        if (result == null) return NotFound(new { error = "Período no encontrado" });
        return Ok(result);
    }
}
