using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using studymate_backend.Libraries.Methods;
using studymate_backend.Libraries.Models;

namespace studymate_backend.Controllers;

[ApiController]
[Route("api/genedgroup/get")]
public class GenedGroupController : ControllerBase
{
    [AllowAnonymous]
    [HttpGet]
    public ActionResult<IEnumerable<GenedGroup>> Get()
    {
        var genedGroups = SdmGenedGroup.GetAll();

        if (genedGroups.Count == 0)
            return NotFound(new { message = "Gened group not found." });
        return Ok(genedGroups);
    }
}