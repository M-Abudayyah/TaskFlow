using Microsoft.AspNetCore.Mvc;

namespace TaskFlow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            success = true,
            message = "API is healthy."
        });
    }
}
