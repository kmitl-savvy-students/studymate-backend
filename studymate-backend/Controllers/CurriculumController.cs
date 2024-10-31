using Microsoft.AspNetCore.Mvc;
using studymate_backend.Libraries.Methods;
using studymate_backend.Libraries.Models;

namespace studymate_backend.Controllers;

[ApiController]
[Route("api/curriculum/get")]
public class CurriculumController : ControllerBase
{
    [HttpGet]
    public ActionResult<IEnumerable<Curriculum>> Get()
    {
        var curriculums = SdmCurriculum.getAll();

        if (curriculums.Count == 0)
            return NotFound("Curriculum not found.");
        return Ok(curriculums);
    }
    [HttpGet("{id:int}")]
    public ActionResult<Curriculum> Get(int id)
    {
        var curriculum = SdmCurriculum.getById(id);

        if (curriculum == null)
            return NotFound("Curriculum not found.");
        return Ok(curriculum);
    }
}
