using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using studymate_backend.Libraries.Methods;
using studymate_backend.Libraries.Models;

namespace studymate_backend.Controllers;

[ApiController]
[Route("api/gened-group/get")]
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

    [AllowAnonymous]
    [HttpGet("{groupId}")]
    public ActionResult<GenedGroup> Get(string groupId)
    {
        var genedGroup = SdmGenedGroup.GetBy(groupId);

        if (genedGroup == null)
            return NotFound(new { message = "Gened group not found." });
        return Ok(genedGroup);
    }
}
