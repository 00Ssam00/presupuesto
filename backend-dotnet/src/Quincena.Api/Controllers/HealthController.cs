using Microsoft.AspNetCore.Mvc;

namespace Quincena.Api.Controllers;

[ApiController]
[Route("api/health")]
public class HealthController : ControllerBase
{
    public record HealthResponse(string Status, DateTime Timestamp);

    [HttpGet]
    public ActionResult<HealthResponse> Get()
    {
        return Ok(new HealthResponse("ok", DateTime.UtcNow));
    }
}
