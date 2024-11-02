using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using studymate_backend.Libraries.Methods;
using studymate_backend.Libraries.Models;

namespace studymate_backend.Controllers;

[ApiController]
[Route("api/curriculum-subgroup/get")]
public class CurriculumSubgroupController : ControllerBase
{
    [AllowAnonymous]
    [HttpGet("{categoryId:int}/{groupId:int}/{subgroupId:int}/{uniqueId}/{year}")]
    public ActionResult<CurriculumSubgroup> GetBy(int categoryId, int groupId, int subgroupId, string uniqueId, string year)
    {
        var curriculumSubgroup = SdmCurriculumSubgroup.GetBy(categoryId, groupId, subgroupId, uniqueId, year);

        if (curriculumSubgroup == null)
            return NotFound(new { message = "Curriculum subgroup not found." });
        return Ok(curriculumSubgroup);
    }
}
