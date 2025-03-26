using Microsoft.AspNetCore.Mvc;
using studymate_backend.Libraries.Database;

namespace studymate_backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DatabaseController : ControllerBase
{
    [HttpGet("status")]
    public IActionResult TestConnection()
    {
        return SdmDataSource.Get() == null ? StatusCode(500) : Ok();
    }
}