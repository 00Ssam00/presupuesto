using Microsoft.AspNetCore.Mvc;
using Quincena.Application.Dashboard;

namespace Quincena.Api.Controllers;

// Port de backend/src/controllers/dashboardController.ts.
[ApiController]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _service;

    public DashboardController(IDashboardService service)
    {
        _service = service;
    }

    [HttpGet("{periodId:guid}")]
    public async Task<IActionResult> Get(Guid periodId)
    {
        var result = await _service.GetAsync(periodId);
        // Mensaje de error propio de este endpoint ("Período no encontrado"),
        // distinto del "No encontrado" genérico del resto de la app — así lo tiene Node.
        if (result == null) return NotFound(new { error = "Período no encontrado" });
        return Ok(result);
    }
}
