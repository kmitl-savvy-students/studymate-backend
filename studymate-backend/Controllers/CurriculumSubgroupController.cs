using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using studymate_backend.Libraries.Methods;
using studymate_backend.Libraries.Models;

namespace studymate_backend.Controllers;

[ApiController]
[Route("api/curriculum-subgroup")]
public class CurriculumSubgroupController : ControllerBase
{
    [AllowAnonymous]
    [HttpGet("get/{categoryId:int}/{groupId:int}/{subgroupId:int}/{uniqueId}/{year}")]
    public ActionResult<CurriculumSubgroup> GetBy(int categoryId, int groupId, int subgroupId, string uniqueId, string year)
    {
        var curriculumSubgroup = SdmCurriculumSubgroup.GetBy(categoryId, groupId, subgroupId, uniqueId, year);

        if (curriculumSubgroup == null)
            return NotFound(new { message = "Curriculum subgroup not found." });
        return Ok(curriculumSubgroup);
    }

    [AllowAnonymous]
    [HttpGet("get/{uniqueId}/{year}")]
    public ActionResult<IEnumerable<CurriculumSubgroup>> GetAllBy(string uniqueId, string year)
    {
        var curriculumSubgroups = SdmCurriculumSubgroup.GetAllBy(uniqueId, year);

        if (curriculumSubgroups.Count == 0)
            return NotFound(new { message = "Curriculum subgroup not found." });
        return Ok(curriculumSubgroups);
    }
    
    [AllowAnonymous]
    [HttpGet("query/{uniqueId}/{year}/{categoryId}/{groupId}")]
    public ActionResult<List<CurriculumSubgroup>> QueryBy(string uniqueId, string year, string categoryId, string groupId)
    {
        var curriculumSubgroups = SdmCurriculumSubgroup.QueryBy(uniqueId, year, categoryId, groupId);
        return Ok(curriculumSubgroups);
    }
}
