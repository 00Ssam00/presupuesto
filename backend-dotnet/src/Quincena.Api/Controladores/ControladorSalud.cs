using Microsoft.AspNetCore.Mvc;

namespace Quincena.Api.Controladores;

[ApiController]
[Route("api/health")]
public class ControladorSalud : ControllerBase
{
    public record RespuestaSalud(string Estado, DateTime Timestamp);

    [HttpGet]
    public ActionResult<RespuestaSalud> Get()
    {
        return Ok(new RespuestaSalud("ok", DateTime.UtcNow));
    }
}
