using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ProjectManagementApp.Api.Controllers;

// Exists solely to exercise the 401/403/200 role matrix in tests (T078/T086) — no business
// meaning of its own.
[ApiController]
[Route("api/admin-probe")]
public class AdminProbeController : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public IActionResult Get() => Ok();
}
