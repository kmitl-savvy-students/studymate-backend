using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using studymate_backend.Libraries.Methods;
using studymate_backend.Libraries.Models;

namespace studymate_backend.Controllers;

[ApiController]
[Route("api/genedsubject/get")]
public class GenedSubjectController : ControllerBase
{
    [AllowAnonymous]
    [HttpGet]
    public ActionResult<IEnumerable<GenedSubject>> Get()
    {
        var genedSubjetcs = SdmGenedSubject.GetAll();

        if (genedSubjetcs.Count == 0)
            return NotFound(new { message = "Gened subject not found." });
        return Ok(genedSubjetcs);
    }
}