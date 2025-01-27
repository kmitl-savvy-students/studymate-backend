using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using studymate_backend.Libraries.Methods;
using studymate_backend.Libraries.Models;

namespace studymate_backend.Controllers;

[ApiController]
[Route("api/curriculum")]
public class CurriculumController : ControllerBase
{
    [AllowAnonymous]
    [HttpGet("get")]
    public ActionResult<IEnumerable<Curriculum>> GetAll()
    {
        var curriculums = SdmCurriculum.GetAll();

        if (curriculums.Count == 0)
            return NotFound(new { message = "Curriculum not found." });
        return Ok(curriculums);
    }

    [AllowAnonymous]
    [HttpGet("get/{id:int}")]
    public ActionResult<Curriculum> GetBy(int id)
    {
        var curriculum = SdmCurriculum.GetBy(id);

        if (curriculum == null)
            return NotFound(new { message = "Curriculum not found." });
        return Ok(curriculum);
    }
    
    [AllowAnonymous]
    [HttpGet("get/{uniqueId}/{year}")]
    public ActionResult<Curriculum> GetBy(string uniqueId, string year)
    {
        var curriculum = SdmCurriculum.GetBy(uniqueId, year);

        if (curriculum == null)
            return NotFound(new { message = "Curriculum not found." });
        return Ok(curriculum);
    }
    
    [AllowAnonymous]
    [HttpGet("query/{uniqueId}/{year}")]
    public ActionResult<Curriculum> QueryBy(string uniqueId, string year)
    {
        var curriculum = SdmCurriculum.QueryBy(uniqueId, year);

        if (curriculum == null)
            return NotFound(new { message = "Curriculum not found." });
        return Ok(curriculum);
    }
}
