using Microsoft.AspNetCore.Mvc;
using studymate_backend.Libraries.Methods;
using studymate_backend.Libraries.Models;

namespace studymate_backend.Controllers;

[ApiController]
[Route("api/curriculumgroup/get")]
public class CurriculumGroupController : ControllerBase
{
    [HttpGet]
    public ActionResult<IEnumerable<CurriculumGroup>> Get()
    {
        var curriculumGroups = SdmCurriculumGroup.getAll();

        if (curriculumGroups.Count == 0)
            return NotFound("Curriculum group not found.");
        return Ok(curriculumGroups);
    }

    [HttpGet("{c_cat_id:int}/{c_group_id:int}")]
    public ActionResult<CurriculumGroup> Get(int c_cat_id, int c_group_id)
    {
        var curriculumGroup = SdmCurriculumGroup.getByCatIdAndGroupId(c_cat_id, c_group_id);

        if (curriculumGroup == null)
            return NotFound("Curriculum group not found.");
        return Ok(curriculumGroup);
    }
}