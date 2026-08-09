using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ProjectManagementApp.Api.Controllers;

[ApiController]
[Route("api/health")]
public class HealthController : ControllerBase
{
    public sealed record HealthResponse(string Status, string Version);

    [HttpGet]
    [AllowAnonymous]
    public ActionResult<HealthResponse> Get() => Ok(new HealthResponse("Healthy", "1.0.0"));
}
