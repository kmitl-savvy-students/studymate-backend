using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using studymate_backend.Libraries.Methods;
using studymate_backend.Libraries.Models;

namespace studymate_backend.Controllers;

[ApiController]
[Route("api/curriculum-group/get")]
public class CurriculumGroupController : ControllerBase
{
    [AllowAnonymous]
    [HttpGet("{categoryId:int}/{groupId:int}/{uniqueId}/{year}")]
    public ActionResult<CurriculumGroup> GetBy(int categoryId, int groupId, string uniqueId, string year)
    {
        var curriculumGroup = SdmCurriculumGroup.GetBy(categoryId, groupId, uniqueId, year);

        if (curriculumGroup == null)
            return NotFound(new { message = "Curriculum group not found." });
        return Ok(curriculumGroup);
    }

    [AllowAnonymous]
    [HttpGet("{uniqueId}/{year}")]
    public ActionResult<IEnumerable<CurriculumGroup>> GetAllBy(string uniqueId, string year)
    {
        var curriculumGroups = SdmCurriculumGroup.GetAllBy(uniqueId, year);

        if (curriculumGroups.Count == 0)
            return NotFound(new { message = "Curriculum group not found." });
        return Ok(curriculumGroups);
    }
}
